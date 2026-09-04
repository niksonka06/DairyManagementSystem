using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DairyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserSocietyFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SocietyID",
                table: "AspNetUsers",
                column: "SocietyID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Societies_SocietyID",
                table: "AspNetUsers",
                column: "SocietyID",
                principalTable: "Societies",
                principalColumn: "SocietyID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Societies_SocietyID",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SocietyID",
                table: "AspNetUsers");
        }
    }
}
