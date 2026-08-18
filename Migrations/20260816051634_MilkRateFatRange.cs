using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class MilkRateFatRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MilkRates_SocietyID_FatPercent_EffectiveFrom",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_FatPercent",
                table: "MilkRates");

            migrationBuilder.RenameColumn(
                name: "FatPercent",
                table: "MilkRates",
                newName: "FatPercentFrom");

            migrationBuilder.AddColumn<decimal>(
                name: "FatPercentTo",
                table: "MilkRates",
                type: "decimal(4,2)",
                nullable: true);

            // Convert old single-fat rows into inclusive bands: each row
            // covers from its fat% up to just below the next band (or 9.00).
            migrationBuilder.Sql(@"
UPDATE m
SET FatPercentTo = CASE
    WHEN r.NextFrom IS NULL THEN CAST(9.00 AS decimal(4,2))
    ELSE CAST(r.NextFrom - 0.01 AS decimal(4,2))
END
FROM MilkRates m
INNER JOIN (
    SELECT RateID,
           LEAD(FatPercentFrom) OVER (PARTITION BY SocietyID, EffectiveFrom ORDER BY FatPercentFrom) AS NextFrom
    FROM MilkRates
) r ON m.RateID = r.RateID;
");

            migrationBuilder.AlterColumn<decimal>(
                name: "FatPercentTo",
                table: "MilkRates",
                type: "decimal(4,2)",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_MilkRates_SocietyID_FatPercentFrom_FatPercentTo_EffectiveFrom",
                table: "MilkRates",
                columns: new[] { "SocietyID", "FatPercentFrom", "FatPercentTo", "EffectiveFrom" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_FatPercentFrom",
                table: "MilkRates",
                sql: "[FatPercentFrom] BETWEEN 2.5 AND 9.0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_FatPercentTo",
                table: "MilkRates",
                sql: "[FatPercentTo] BETWEEN 2.5 AND 9.0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_FatRange",
                table: "MilkRates",
                sql: "[FatPercentFrom] <= [FatPercentTo]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MilkRates_SocietyID_FatPercentFrom_FatPercentTo_EffectiveFrom",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_FatPercentFrom",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_FatPercentTo",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_FatRange",
                table: "MilkRates");

            migrationBuilder.DropColumn(
                name: "FatPercentTo",
                table: "MilkRates");

            migrationBuilder.RenameColumn(
                name: "FatPercentFrom",
                table: "MilkRates",
                newName: "FatPercent");

            migrationBuilder.CreateIndex(
                name: "IX_MilkRates_SocietyID_FatPercent_EffectiveFrom",
                table: "MilkRates",
                columns: new[] { "SocietyID", "FatPercent", "EffectiveFrom" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_FatPercent",
                table: "MilkRates",
                sql: "[FatPercent] BETWEEN 2.5 AND 9.0");
        }
    }
}
