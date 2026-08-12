using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMilkCollectionFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MilkCollections",
                columns: table => new
                {
                    CollectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerID = table.Column<int>(type: "int", nullable: false),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "date", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    FatPercent = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    SNF = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    CLR = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RatePerLitre = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RecordedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedBySettlementID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkCollections", x => x.CollectionID);
                    table.CheckConstraint("CK_MilkCollections_FatPercent", "[FatPercent] BETWEEN 2.5 AND 9.0");
                    table.CheckConstraint("CK_MilkCollections_Quantity", "[Quantity] BETWEEN 0.5 AND 500");
                    table.CheckConstraint("CK_MilkCollections_RatePerLitre", "[RatePerLitre] > 0");
                    table.CheckConstraint("CK_MilkCollections_SNF", "[SNF] IS NULL OR [SNF] BETWEEN 7.5 AND 11.0");
                    table.ForeignKey(
                        name: "FK_MilkCollections_AspNetUsers_RecordedBy",
                        column: x => x.RecordedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilkCollections_Farmers_FarmerID",
                        column: x => x.FarmerID,
                        principalTable: "Farmers",
                        principalColumn: "FarmerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilkCollections_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_FarmerID_CollectionDate_Shift",
                table: "MilkCollections",
                columns: new[] { "FarmerID", "CollectionDate", "Shift" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_RecordedBy",
                table: "MilkCollections",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_SocietyID",
                table: "MilkCollections",
                column: "SocietyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MilkCollections");
        }
    }
}
