using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class CollectionRejectionShiftCloseAuditSociety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkCollections_RatePerLitre",
                table: "MilkCollections");

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "MilkCollections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "MilkCollections",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SocietyID",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE a
                SET SocietyID = u.SocietyID
                FROM AuditLogs a
                INNER JOIN AspNetUsers u ON a.PerformedBy = u.Id
                WHERE a.SocietyID IS NULL AND u.SocietyID IS NOT NULL;
                """);

            migrationBuilder.CreateTable(
                name: "ShiftCloses",
                columns: table => new
                {
                    ShiftCloseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "date", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClosedBy = table.Column<int>(type: "int", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftCloses", x => x.ShiftCloseID);
                    table.ForeignKey(
                        name: "FK_ShiftCloses_AspNetUsers_ClosedBy",
                        column: x => x.ClosedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftCloses_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkCollections_RatePerLitre",
                table: "MilkCollections",
                sql: "[RatePerLitre] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkCollections_Rejection",
                table: "MilkCollections",
                sql: "([IsRejected] = 0 AND [RatePerLitre] > 0) OR ([IsRejected] = 1 AND [RatePerLitre] = 0 AND [Amount] = 0)");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SocietyID",
                table: "AuditLogs",
                column: "SocietyID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCloses_ClosedBy",
                table: "ShiftCloses",
                column: "ClosedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCloses_SocietyID_CollectionDate_Shift",
                table: "ShiftCloses",
                columns: new[] { "SocietyID", "CollectionDate", "Shift" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Societies_SocietyID",
                table: "AuditLogs",
                column: "SocietyID",
                principalTable: "Societies",
                principalColumn: "SocietyID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Societies_SocietyID",
                table: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ShiftCloses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkCollections_RatePerLitre",
                table: "MilkCollections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilkCollections_Rejection",
                table: "MilkCollections");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_SocietyID",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "MilkCollections");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "MilkCollections");

            migrationBuilder.DropColumn(
                name: "SocietyID",
                table: "AuditLogs");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilkCollections_RatePerLitre",
                table: "MilkCollections",
                sql: "[RatePerLitre] > 0");
        }
    }
}
