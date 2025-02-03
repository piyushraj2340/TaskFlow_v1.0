using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnTaskId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todo_Tasks_TaskIdId",
                table: "Todo");

            migrationBuilder.RenameColumn(
                name: "TaskIdId",
                table: "Todo",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Todo_TaskIdId",
                table: "Todo",
                newName: "IX_Todo_TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_Tasks_TaskId",
                table: "Todo",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todo_Tasks_TaskId",
                table: "Todo");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Todo",
                newName: "TaskIdId");

            migrationBuilder.RenameIndex(
                name: "IX_Todo_TaskId",
                table: "Todo",
                newName: "IX_Todo_TaskIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_Tasks_TaskIdId",
                table: "Todo",
                column: "TaskIdId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
