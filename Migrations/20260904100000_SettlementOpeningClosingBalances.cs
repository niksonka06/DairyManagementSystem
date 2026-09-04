using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    [DbContext(typeof(Data.ApplicationDbContext))]
    [Migration("20260904100000_SettlementOpeningClosingBalances")]
    public partial class SettlementOpeningClosingBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ClosingBalance",
                table: "Payments",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "Payments",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE Payments
                SET OpeningBalance = PreviousDue,
                    ClosingBalance = NetAmount;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingBalance",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "Payments");
        }
    }
}
