using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstrainOnTodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Todo_EndDate_TaskId",
                table: "Todo",
                columns: new[] { "EndDate", "TaskId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Todo_EndDate_TaskId",
                table: "Todo");
        }
    }
}
