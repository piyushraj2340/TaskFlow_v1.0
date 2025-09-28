using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class AddManualTodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManualAdded",
                table: "Todo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManualAdded",
                table: "Todo");
        }
    }
}
