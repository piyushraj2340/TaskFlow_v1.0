using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueEmailValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true); // Add this line to enforce uniqueness
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");
        }

    }
}
