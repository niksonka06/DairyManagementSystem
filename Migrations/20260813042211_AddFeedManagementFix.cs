using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedManagementFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedInventoryItems",
                columns: table => new
                {
                    FeedItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FeedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LowStockThreshold = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedInventoryItems", x => x.FeedItemID);
                    table.CheckConstraint("CK_FeedInventory_PricePerUnit", "[PricePerUnit] > 0");
                    table.CheckConstraint("CK_FeedInventory_StockQuantity", "[StockQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_FeedInventoryItems_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedIssues",
                columns: table => new
                {
                    IssueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedItemID = table.Column<int>(type: "int", nullable: false),
                    FarmerID = table.Column<int>(type: "int", nullable: false),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitPriceAtIssue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedBySettlementID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedIssues", x => x.IssueID);
                    table.CheckConstraint("CK_FeedIssues_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_FeedIssues_AspNetUsers_IssuedBy",
                        column: x => x.IssuedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedIssues_FeedInventoryItems_FeedItemID",
                        column: x => x.FeedItemID,
                        principalTable: "FeedInventoryItems",
                        principalColumn: "FeedItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedIssues_Farmers_FarmerID",
                        column: x => x.FarmerID,
                        principalTable: "Farmers",
                        principalColumn: "FarmerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedIssues_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedInventoryItems_SocietyID_ItemType_FeedName",
                table: "FeedInventoryItems",
                columns: new[] { "SocietyID", "ItemType", "FeedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedIssues_FeedItemID",
                table: "FeedIssues",
                column: "FeedItemID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedIssues_FarmerID",
                table: "FeedIssues",
                column: "FarmerID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedIssues_SocietyID",
                table: "FeedIssues",
                column: "SocietyID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedIssues_IssuedBy",
                table: "FeedIssues",
                column: "IssuedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FeedIssues");
            migrationBuilder.DropTable(name: "FeedInventoryItems");
        }
    }
}