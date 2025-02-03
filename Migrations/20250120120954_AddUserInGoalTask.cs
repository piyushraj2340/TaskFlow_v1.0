using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInGoalTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "GoalTasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalTasks_UserId",
                table: "GoalTasks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalTasks_AspNetUsers_UserId",
                table: "GoalTasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalTasks_AspNetUsers_UserId",
                table: "GoalTasks");

            migrationBuilder.DropIndex(
                name: "IX_GoalTasks_UserId",
                table: "GoalTasks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GoalTasks");
        }
    }
}
