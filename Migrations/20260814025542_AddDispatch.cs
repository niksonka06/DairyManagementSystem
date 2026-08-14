using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispatches",
                columns: table => new
                {
                    DispatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocietyID = table.Column<int>(type: "int", nullable: false),
                    DispatchDate = table.Column<DateTime>(type: "date", nullable: false),
                    DispatchTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    VehicleNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalCollected = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalDispatched = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VarianceReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecordedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatches", x => x.DispatchID);
                    table.CheckConstraint("CK_Dispatches_TotalDispatched", "[TotalDispatched] > 0");
                    table.ForeignKey(
                        name: "FK_Dispatches_AspNetUsers_RecordedBy",
                        column: x => x.RecordedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dispatches_Societies_SocietyID",
                        column: x => x.SocietyID,
                        principalTable: "Societies",
                        principalColumn: "SocietyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_RecordedBy",
                table: "Dispatches",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_SocietyID_DispatchDate",
                table: "Dispatches",
                columns: new[] { "SocietyID", "DispatchDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dispatches");
        }
    }
}
