using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Controllers
{
    // Deliberately NOT inside Areas/Admin or Areas/Operator — both roles use
    // the exact same report screens, just scoped differently (see
    // ResolveSocietyIdAsync). Building this twice under each Area would be
    // the same "duplicated logic" the coding standards warn against.
    [Authorize(Roles = "Admin,Operator")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ISocietyService _societyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(IReportService reportService, ISocietyService societyService, UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _societyService = societyService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? societyId, CancellationToken ct)
        {
            if (User.IsInRole(Roles.Admin))
            {
                ViewBag.Societies = await _societyService.GetAllAsync(ct);
                ViewBag.SelectedSocietyId = societyId;
            }
            return View();
        }

        public async Task<IActionResult> DailyCollection(int? societyId, DateTime? date, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var model = await _reportService.GetDailyCollectionReportAsync(resolvedSocietyId.Value, date ?? DateTime.Today, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> DailyCollectionPdf(int? societyId, DateTime? date, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var m = await _reportService.GetDailyCollectionReportAsync(resolvedSocietyId.Value, date ?? DateTime.Today, ct);
            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Summary",
                    Headers = new[] { "Metric", "Value" },
                    Rows = new List<string[]>
                    {
                        new[] { "Total Quantity", $"{m.TotalQuantity:0.00} L" },
                        new[] { "Total Amount", $"Rs.{m.TotalAmount:0.00}" },
                        new[] { "Farmer Count", $"{m.FarmerCount}" },
                        new[] { "Average Fat %", $"{m.AverageFatPercent:0.00}%" },
                        new[] { "Morning", $"{m.MorningQuantity:0.00} L / Rs.{m.MorningAmount:0.00}" },
                        new[] { "Evening", $"{m.EveningQuantity:0.00} L / Rs.{m.EveningAmount:0.00}" }
                    }
                }
            };
            var pdf = PdfReportGenerator.Generate("Daily Milk Collection Report", m.ReportDate.ToString("dd-MMM-yyyy"), sections);
            return File(pdf, "application/pdf", $"DailyCollection_{m.ReportDate:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> WeeklyCollection(int? societyId, DateTime? date, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var model = await _reportService.GetWeeklyCollectionSummaryAsync(resolvedSocietyId.Value, date ?? DateTime.Today, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> WeeklyCollectionPdf(int? societyId, DateTime? date, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var m = await _reportService.GetWeeklyCollectionSummaryAsync(resolvedSocietyId.Value, date ?? DateTime.Today, ct);
            var section = new PdfSection
            {
                Headers = new[] { "Farmer Code", "Name", "Total Qty (L)", "Avg Fat %", "Gross Amount" },
                Rows = m.Rows.Select(r => new[] { r.FarmerCode, r.FarmerName, r.TotalQuantity.ToString("0.00"), r.AverageFat.ToString("0.00"), $"Rs.{r.GrossAmount:0.00}" }).ToList()
            };
            var pdf = PdfReportGenerator.Generate("Weekly Collection Summary", $"{m.PeriodStart:dd-MMM-yyyy} to {m.PeriodEnd:dd-MMM-yyyy}", new[] { section },
                new[] { $"Total Gross Amount: Rs.{m.Rows.Sum(r => r.GrossAmount):0.00}" });
            return File(pdf, "application/pdf", $"WeeklySummary_{m.PeriodStart:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> FarmerSettlement(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var (f, t) = DefaultRange(from, to);
            var model = await _reportService.GetFarmerSettlementReportAsync(resolvedSocietyId.Value, f, t, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> FarmerSettlementPdf(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var (f, t) = DefaultRange(from, to);
            var m = await _reportService.GetFarmerSettlementReportAsync(resolvedSocietyId.Value, f, t, ct);
            var section = new PdfSection
            {
                Headers = new[] { "Farmer", "Period", "Gross", "Feed", "Medicine", "Other", "Prev Due", "Advance", "Net", "Status" },
                Rows = m.Rows.Select(r => new[]
                {
                    $"{r.FarmerCode} - {r.FarmerName}",
                    $"{r.PeriodStart:dd-MMM} to {r.PeriodEnd:dd-MMM}",
                    r.GrossAmount.ToString("0.00"), r.FeedDeduction.ToString("0.00"), r.MedicineDeduction.ToString("0.00"),
                    r.OtherDeductionsTotal.ToString("0.00"), r.PreviousDue.ToString("0.00"), r.AdvancePaid.ToString("0.00"),
                    r.NetAmount.ToString("0.00"), r.Status.ToString()
                }).ToList()
            };
            var pdf = PdfReportGenerator.Generate("Farmer Settlement Report", $"{f:dd-MMM-yyyy} to {t:dd-MMM-yyyy}", new[] { section },
                new[] { $"Total Net Amount (excl. Cancelled): Rs.{m.TotalNetAmount:0.00}" });
            return File(pdf, "application/pdf", $"SettlementReport_{f:yyyyMMdd}_{t:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> FeedStock(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var (f, t) = DefaultRange(from, to);
            var model = await _reportService.GetFeedStockReportAsync(resolvedSocietyId.Value, f, t, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> FeedStockPdf(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var (f, t) = DefaultRange(from, to);
            var m = await _reportService.GetFeedStockReportAsync(resolvedSocietyId.Value, f, t, ct);
            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Current Stock",
                    Headers = new[] { "Type", "Name", "Unit", "Stock", "Price/Unit" },
                    Rows = m.CurrentStock.Select(r => new[] { r.ItemType.ToString(), r.FeedName, r.Unit, r.StockQuantity.ToString("0.00") + (r.IsLowStock ? " (LOW)" : ""), $"Rs.{r.PricePerUnit:0.00}" }).ToList()
                },
                new PdfSection
                {
                    Title = "Farmer-wise Deduction Totals",
                    Headers = new[] { "Farmer", "Feed Cost", "Medicine Cost" },
                    Rows = m.FarmerDeductionTotals.Select(r => new[] { $"{r.FarmerCode} - {r.FarmerName}", $"Rs.{r.TotalFeedCost:0.00}", $"Rs.{r.TotalMedicineCost:0.00}" }).ToList()
                }
            };
            var pdf = PdfReportGenerator.Generate("Feed Stock Report", $"Issues from {f:dd-MMM-yyyy} to {t:dd-MMM-yyyy}", sections);
            return File(pdf, "application/pdf", $"FeedStock_{DateTime.Today:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> Dispatch(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var (f, t) = DefaultRange(from, to);
            var model = await _reportService.GetDispatchReportAsync(resolvedSocietyId.Value, f, t, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> DispatchPdf(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var (f, t) = DefaultRange(from, to);
            var m = await _reportService.GetDispatchReportAsync(resolvedSocietyId.Value, f, t, ct);
            var section = new PdfSection
            {
                Headers = new[] { "Date", "Vehicle", "Destination", "Collected", "Dispatched", "Variance", "Operator" },
                Rows = m.Rows.Select(r => new[]
                {
                    r.DispatchDate.ToString("dd-MMM-yyyy"), r.VehicleNo, r.Destination,
                    $"{r.TotalCollected:0.00}L", $"{r.TotalDispatched:0.00}L", $"{r.Variance:0.00}L ({r.VariancePercent:0.0}%)", r.OperatorName
                }).ToList()
            };
            var pdf = PdfReportGenerator.Generate("Dispatch Report", $"{f:dd-MMM-yyyy} to {t:dd-MMM-yyyy}", new[] { section });
            return File(pdf, "application/pdf", $"DispatchReport_{f:yyyyMMdd}_{t:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> FatAnalysis(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var (f, t) = DefaultRange(from, to, days: 30);
            var model = await _reportService.GetFatAnalysisReportAsync(resolvedSocietyId.Value, f, t, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> FatAnalysisPdf(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var (f, t) = DefaultRange(from, to, days: 30);
            var m = await _reportService.GetFatAnalysisReportAsync(resolvedSocietyId.Value, f, t, ct);
            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Farmer-wise Average Fat %",
                    Headers = new[] { "Farmer", "Avg Fat %", "Collections" },
                    Rows = m.FarmerAverages.Select(r => new[] { $"{r.FarmerCode} - {r.FarmerName}", r.AverageFat.ToString("0.00"), r.CollectionCount.ToString() }).ToList()
                },
                new PdfSection
                {
                    Title = "Fat Distribution",
                    Headers = new[] { "Range", "Count" },
                    Rows = m.Distribution.Select(r => new[] { r.Label, r.Count.ToString() }).ToList()
                }
            };
            var pdf = PdfReportGenerator.Generate("Fat Analysis Report", $"{f:dd-MMM-yyyy} to {t:dd-MMM-yyyy}", sections);
            return File(pdf, "application/pdf", $"FatAnalysis_{f:yyyyMMdd}_{t:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> TopSuppliers(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return RedirectToAction(nameof(Index));

            var (f, t) = DefaultRange(from, to, days: 30);
            var model = await _reportService.GetTopSuppliersReportAsync(resolvedSocietyId.Value, f, t, ct);
            ViewBag.SocietyId = resolvedSocietyId;
            return View(model);
        }

        public async Task<IActionResult> TopSuppliersPdf(int? societyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var resolvedSocietyId = await ResolveSocietyIdAsync(societyId, ct);
            if (resolvedSocietyId is null) return BadRequest("Society not resolved.");

            var (f, t) = DefaultRange(from, to, days: 30);
            var m = await _reportService.GetTopSuppliersReportAsync(resolvedSocietyId.Value, f, t, ct);
            var sections = new List<PdfSection>
            {
                new PdfSection { Title = "Top 10 by Quantity", Headers = new[] { "Farmer", "Total Quantity (L)" },
                    Rows = m.TopByQuantity.Select(r => new[] { $"{r.FarmerCode} - {r.FarmerName}", r.Value.ToString("0.00") }).ToList() },
                new PdfSection { Title = "Top 10 by Income", Headers = new[] { "Farmer", "Total Income" },
                    Rows = m.TopByIncome.Select(r => new[] { $"{r.FarmerCode} - {r.FarmerName}", $"Rs.{r.Value:0.00}" }).ToList() }
            };
            var pdf = PdfReportGenerator.Generate("Top Suppliers Report", $"{f:dd-MMM-yyyy} to {t:dd-MMM-yyyy}", sections);
            return File(pdf, "application/pdf", $"TopSuppliers_{f:yyyyMMdd}_{t:yyyyMMdd}.pdf");
        }

        // ── Shared helpers ──────────────────────────────────────────────

        private static (DateTime From, DateTime To) DefaultRange(DateTime? from, DateTime? to, int days = 30)
        {
            var effectiveTo = (to ?? DateTime.Today).Date;
            var effectiveFrom = (from ?? effectiveTo.AddDays(-days)).Date;
            return (effectiveFrom, effectiveTo);
        }

        // Operator: always their own society, never trusts a query-string
        // override (an Operator passing ?societyId=5 must NOT be able to
        // view another society's reports). Admin: must explicitly pick a
        // society via the query string (no default), since Admin isn't
        // scoped to one.
        private async Task<int?> ResolveSocietyIdAsync(int? requestedSocietyId, CancellationToken ct)
        {
            if (User.IsInRole(Roles.Operator))
            {
                var user = await _userManager.GetUserAsync(User);
                return user?.SocietyID;
            }

            // Admin — only trust the query string society id if provided.
            return requestedSocietyId;
        }
    }
}
