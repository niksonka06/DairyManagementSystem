using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvancePaid = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FarmerID = table.Column<int>(type: "int", nullable: false),
                    FeedDeduction = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedBy = table.Column<int>(type: "int", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MedicineDeduction = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    OtherDeductionsTotal = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "date", nullable: false),
                    PreviousDue = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_GeneratedBy",
                        column: x => x.GeneratedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Farmers_FarmerID",
                        column: x => x.FarmerID,
                        principalTable: "Farmers",
                        principalColumn: "FarmerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GeneratedBy",
                table: "Payments",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SocietyID",
                table: "Payments",
                column: "SocietyID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FarmerID_PeriodStart",
                table: "Payments",
                columns: new[] { "FarmerID", "PeriodStart" },
                unique: true,
                filter: "[Status] <> 'Cancelled'");

            migrationBuilder.CreateTable(
                name: "SettlementDeductions",
                columns: table => new
                {
                    SettlementDeductionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DeductionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementDeductions", x => x.SettlementDeductionID);
                    table.ForeignKey(
                        name: "FK_SettlementDeductions_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID",
                        onDelete: ReferentialAction.Cascade);
                    table.CheckConstraint("CK_SettlementDeductions_Amount", "[Amount] > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDeductions_PaymentID",
                table: "SettlementDeductions",
                column: "PaymentID");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "SettlementDeductions");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
