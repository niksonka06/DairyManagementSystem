using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMilkRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MilkRates",
                columns: table => new
                {
                    RateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    FatPercent = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    RatePerLitre = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkRates", x => x.RateID);
                    table.CheckConstraint("CK_MilkRates_FatPercent", "[FatPercent] BETWEEN 2.5 AND 9.0");
                    table.CheckConstraint("CK_MilkRates_RatePerLitre", "[RatePerLitre] > 0");
                    table.ForeignKey(
                        name: "FK_MilkRates_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MilkRates_SocietyID_FatPercent_EffectiveFrom",
                table: "MilkRates",
                columns: new[] { "SocietyID", "FatPercent", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MilkRates");
        }
    }
}
