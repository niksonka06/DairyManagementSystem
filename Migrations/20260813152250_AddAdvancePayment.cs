using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvancePayments",
                columns: table => new
                {
                    AdvancePaymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerID = table.Column<int>(type: "int", nullable: false),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "date", nullable: false),
                    RecordedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApplied = table.Column<bool>(type: "bit", nullable: false),
                    AppliedToPaymentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancePayments", x => x.AdvancePaymentID);
                    table.CheckConstraint("CK_AdvancePayments_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_AdvancePayments_AspNetUsers_RecordedBy",
                        column: x => x.RecordedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancePayments_Farmers_FarmerID",
                        column: x => x.FarmerID,
                        principalTable: "Farmers",
                        principalColumn: "FarmerID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePayments_FarmerID",
                table: "AdvancePayments",
                column: "FarmerID");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePayments_RecordedBy",
                table: "AdvancePayments",
                column: "RecordedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancePayments");
        }
    }
}
