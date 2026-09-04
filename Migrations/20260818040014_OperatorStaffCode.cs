using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class OperatorStaffCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffCode",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StaffCode",
                table: "AspNetUsers",
                column: "StaffCode",
                unique: true,
                filter: "[StaffCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_StaffCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StaffCode",
                table: "AspNetUsers");
        }
    }
}
