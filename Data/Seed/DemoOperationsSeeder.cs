using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Data.Seed
{
    // Fills daily operational rows against masters that already exist:
    // societies, farmers, rate charts, operators, feed stock.
    // Does not create societies, farmers, rates, or users.
    public static class DemoOperationsSeeder
    {
        private const int DaysToFill = 14;
        private const double PourChance = 0.75;
        private const double FeedIssueChance = 0.18;

        public static async Task SeedAsync(IServiceProvider services)
        {
            var config = services.GetRequiredService<IConfiguration>();
            var env = services.GetRequiredService<IHostEnvironment>();
            var enabled = config.GetValue("AppSettings:SeedDemoOperations", env.IsDevelopment());
            if (!enabled)
            {
                return;
            }

            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DemoOperationsSeeder");

            var societies = await db.Societies.AsNoTracking()
                .Where(s => s.IsActive)
                .Select(s => s.SocietyID)
                .ToListAsync();

            if (societies.Count == 0)
            {
                logger.LogInformation("Demo operations seed skipped: no societies.");
                return;
            }

            var admin = await userManager.FindByEmailAsync("admin@dairysystem.local");
            var operators = await userManager.GetUsersInRoleAsync(Roles.Operator);

            var rng = new Random(42);
            var from = DateTime.Today.AddDays(-(DaysToFill - 1)).Date;
            var to = DateTime.Today.Date;

            var collectionsAdded = 0;
            var dispatchesAdded = 0;
            var issuesAdded = 0;
            var advancesAdded = 0;

            foreach (var societyId in societies)
            {
                var farmers = await db.Farmers
                    .Where(f => f.SocietyID == societyId && f.IsActive)
                    .Select(f => f.FarmerID)
                    .ToListAsync();

                if (farmers.Count == 0)
                {
                    logger.LogInformation("Demo operations seed skipped for society {SocietyId}: no farmers.", societyId);
                    continue;
                }

                var rates = await db.MilkRates
                    .Where(r => r.SocietyID == societyId && r.IsActive)
                    .ToListAsync();

                if (rates.Count == 0)
                {
                    logger.LogWarning("Demo operations seed skipped collections for society {SocietyId}: no milk rates.", societyId);
                    continue;
                }

                var recordedBy = operators.FirstOrDefault(o => o.SocietyID == societyId)?.Id
                    ?? admin?.Id
                    ?? operators.FirstOrDefault()?.Id;

                if (recordedBy is null)
                {
                    logger.LogWarning("Demo operations seed skipped society {SocietyId}: no operator or admin to record against.", societyId);
                    continue;
                }

                var existingKeys = (await db.MilkCollections
                        .Where(c => c.SocietyID == societyId && c.CollectionDate >= from && c.CollectionDate <= to)
                        .Select(c => new { c.FarmerID, c.CollectionDate, c.Shift })
                        .ToListAsync())
                    .Select(c => (c.FarmerID, c.CollectionDate, c.Shift))
                    .ToHashSet();

                var existingDispatchDates = (await db.Dispatches
                        .Where(d => d.SocietyID == societyId && d.DispatchDate >= from && d.DispatchDate <= to)
                        .Select(d => d.DispatchDate)
                        .ToListAsync())
                    .Select(d => d.Date)
                    .ToHashSet();

                var newCollections = new List<MilkCollection>();

                for (var day = from; day <= to; day = day.AddDays(1))
                {
                    foreach (var shift in new[] { Shift.Morning, Shift.Evening })
                    {
                        foreach (var farmerId in farmers)
                        {
                            if (existingKeys.Contains((farmerId, day, shift)))
                            {
                                continue;
                            }

                            if (rng.NextDouble() > PourChance)
                            {
                                continue;
                            }

                            var rate = rates[rng.Next(rates.Count)];
                            var fat = NextInRange(rng, rate.FatPercentFrom, rate.FatPercentTo);
                            var snf = NextInRange(rng, rate.SnfPercentFrom, rate.SnfPercentTo);
                            var clr = NextInRange(rng, rate.ClrFrom, rate.ClrTo);
                            var quantity = 2.0m + (rng.Next(0, 33) * 0.5m); // 2.0–18.0 L
                            var amount = Math.Round(quantity * rate.RatePerLitre, 2);

                            var collection = new MilkCollection
                            {
                                FarmerID = farmerId,
                                SocietyID = societyId,
                                CollectionDate = day,
                                Shift = shift,
                                Quantity = quantity,
                                FatPercent = fat,
                                SNF = snf,
                                CLR = clr,
                                RatePerLitre = rate.RatePerLitre,
                                Amount = amount,
                                RecordedBy = recordedBy.Value,
                                CreatedAt = DateTime.UtcNow,
                                IsLocked = false
                            };

                            newCollections.Add(collection);
                            existingKeys.Add((farmerId, day, shift));
                        }
                    }
                }

                if (newCollections.Count > 0)
                {
                    db.MilkCollections.AddRange(newCollections);
                    await db.SaveChangesAsync();
                    collectionsAdded += newCollections.Count;
                }

                for (var day = from; day <= to; day = day.AddDays(1))
                {
                    if (existingDispatchDates.Contains(day))
                    {
                        continue;
                    }

                    var collected = await db.MilkCollections
                        .Where(c => c.SocietyID == societyId && c.CollectionDate == day)
                        .SumAsync(c => (decimal?)c.Quantity) ?? 0m;

                    if (collected < 0.5m)
                    {
                        continue;
                    }

                    var lossFactor = 0.97m + ((decimal)rng.NextDouble() * 0.03m);
                    var dispatched = Math.Round(collected * lossFactor, 2);
                    if (dispatched < 0.01m)
                    {
                        continue;
                    }

                    var variancePct = collected == 0 ? 0 : Math.Abs(collected - dispatched) / collected * 100;
                    var eveningHeavy = await db.MilkCollections.CountAsync(c =>
                        c.SocietyID == societyId && c.CollectionDate == day && c.Shift == Shift.Evening)
                        > await db.MilkCollections.CountAsync(c =>
                            c.SocietyID == societyId && c.CollectionDate == day && c.Shift == Shift.Morning);

                    db.Dispatches.Add(new Dispatch
                    {
                        SocietyID = societyId,
                        DispatchDate = day,
                        DispatchTime = eveningHeavy ? new TimeSpan(18, 30, 0) : new TimeSpan(7, 30, 0),
                        VehicleNo = $"KL-{rng.Next(1, 15):00}-{rng.Next(1000, 10000)}",
                        Destination = "District Union Dairy",
                        TotalCollected = collected,
                        TotalDispatched = dispatched,
                        VarianceReason = variancePct >= 1m ? "Transit / weighing difference" : null,
                        RecordedBy = recordedBy.Value,
                        CreatedAt = DateTime.UtcNow
                    });

                    existingDispatchDates.Add(day);
                    dispatchesAdded++;
                }

                await db.SaveChangesAsync();

                var stockItems = await db.FeedInventoryItems
                    .Where(i => i.SocietyID == societyId && i.IsActive && i.StockQuantity > 0)
                    .ToListAsync();

                var alreadyHasIssues = await db.FeedIssues.AnyAsync(i =>
                    i.SocietyID == societyId && i.IssueDate >= from && i.IssueDate <= to);

                if (stockItems.Count > 0 && !alreadyHasIssues)
                {
                    for (var day = from; day <= to; day = day.AddDays(1))
                    {
                        foreach (var farmerId in farmers)
                        {
                            if (rng.NextDouble() > FeedIssueChance)
                            {
                                continue;
                            }

                            var item = stockItems[rng.Next(stockItems.Count)];
                            var qty = Math.Min(1m + rng.Next(0, 3), item.StockQuantity);
                            if (qty <= 0)
                            {
                                continue;
                            }

                            item.StockQuantity -= qty;
                            db.FeedIssues.Add(new FeedIssue
                            {
                                FeedItemID = item.FeedItemID,
                                FarmerID = farmerId,
                                SocietyID = societyId,
                                ItemType = item.ItemType,
                                Quantity = qty,
                                UnitPriceAtIssue = item.PricePerUnit,
                                TotalCost = Math.Round(qty * item.PricePerUnit, 2),
                                IssueDate = day,
                                IssuedBy = recordedBy.Value,
                                CreatedAt = DateTime.UtcNow,
                                IsLocked = false
                            });
                            issuesAdded++;
                        }
                    }

                    await db.SaveChangesAsync();
                }

                var hasAdvances = await db.AdvancePayments.AnyAsync(a => a.SocietyID == societyId);
                if (!hasAdvances)
                {
                    var advanceCount = Math.Min(3, farmers.Count);
                    var picks = farmers.OrderBy(_ => rng.Next()).Take(advanceCount).ToList();
                    foreach (var farmerId in picks)
                    {
                        db.AdvancePayments.Add(new AdvancePayment
                        {
                            FarmerID = farmerId,
                            SocietyID = societyId,
                            Amount = 500m + rng.Next(0, 26) * 100m,
                            PaymentDate = from.AddDays(rng.Next(0, DaysToFill)),
                            RecordedBy = recordedBy.Value,
                            CreatedAt = DateTime.UtcNow,
                            IsApplied = false
                        });
                        advancesAdded++;
                    }

                    await db.SaveChangesAsync();
                }
            }

            logger.LogInformation(
                "Demo operations seed: {Collections} collections, {Dispatches} dispatches, {Issues} feed issues, {Advances} advances.",
                collectionsAdded, dispatchesAdded, issuesAdded, advancesAdded);
        }

        private static decimal NextInRange(Random rng, decimal from, decimal to)
        {
            if (to <= from)
            {
                return Math.Round(from, 2);
            }

            var value = from + (to - from) * (decimal)rng.NextDouble();
            var rounded = Math.Round(value, 2);
            if (rounded < from)
            {
                rounded = from;
            }

            if (rounded > to)
            {
                rounded = to;
            }

            return rounded;
        }
    }
}
