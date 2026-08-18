using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class MilkRateQualityBands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MilkRates_SocietyID_FatPercentFrom_FatPercentTo_EffectiveFrom",
                table: "MilkRates");

            migrationBuilder.AddColumn<decimal>(
                name: "ClrFrom",
                table: "MilkRates",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ClrTo",
                table: "MilkRates",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 50m);

            migrationBuilder.AddColumn<decimal>(
                name: "SnfPercentFrom",
                table: "MilkRates",
                type: "decimal(4,2)",
                nullable: false,
                defaultValue: 7.5m);

            migrationBuilder.AddColumn<decimal>(
                name: "SnfPercentTo",
                table: "MilkRates",
                type: "decimal(4,2)",
                nullable: false,
                defaultValue: 11.0m);

            migrationBuilder.CreateIndex(
                name: "IX_MilkRates_SocietyID_EffectiveFrom",
                table: "MilkRates",
                columns: new[] { "SocietyID", "EffectiveFrom" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_ClrFrom",
                table: "MilkRates",
                sql: "[ClrFrom] BETWEEN 0 AND 50");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_ClrRange",
                table: "MilkRates",
                sql: "[ClrFrom] <= [ClrTo]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_ClrTo",
                table: "MilkRates",
                sql: "[ClrTo] BETWEEN 0 AND 50");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_SnfPercentFrom",
                table: "MilkRates",
                sql: "[SnfPercentFrom] BETWEEN 7.5 AND 11.0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_SnfPercentTo",
                table: "MilkRates",
                sql: "[SnfPercentTo] BETWEEN 7.5 AND 11.0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkRates_SnfRange",
                table: "MilkRates",
                sql: "[SnfPercentFrom] <= [SnfPercentTo]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MilkRates_SocietyID_EffectiveFrom",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_ClrFrom",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_ClrRange",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_ClrTo",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_SnfPercentFrom",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_SnfPercentTo",
                table: "MilkRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkRates_SnfRange",
                table: "MilkRates");

            migrationBuilder.DropColumn(
                name: "ClrFrom",
                table: "MilkRates");

            migrationBuilder.DropColumn(
                name: "ClrTo",
                table: "MilkRates");

            migrationBuilder.DropColumn(
                name: "SnfPercentFrom",
                table: "MilkRates");

            migrationBuilder.DropColumn(
                name: "SnfPercentTo",
                table: "MilkRates");

            migrationBuilder.CreateIndex(
                name: "IX_MilkRates_SocietyID_FatPercentFrom_FatPercentTo_EffectiveFrom",
                table: "MilkRates",
                columns: new[] { "SocietyID", "FatPercentFrom", "FatPercentTo", "EffectiveFrom" },
                unique: true);
        }
    }
}
