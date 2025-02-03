using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class AlterGoalAndTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalsTasks");

            migrationBuilder.AddColumn<int>(
                name: "TasksId",
                table: "Goals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_TasksId",
                table: "Goals",
                column: "TasksId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Tasks_TasksId",
                table: "Goals",
                column: "TasksId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Tasks_TasksId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_TasksId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "TasksId",
                table: "Goals");

            migrationBuilder.CreateTable(
                name: "GoalsTasks",
                columns: table => new
                {
                    GoalsListId = table.Column<int>(type: "int", nullable: false),
                    TasksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalsTasks", x => new { x.GoalsListId, x.TasksId });
                    table.ForeignKey(
                        name: "FK_GoalsTasks_Goals_GoalsListId",
                        column: x => x.GoalsListId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalsTasks_Tasks_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalsTasks_TasksId",
                table: "GoalsTasks",
                column: "TasksId");
        }
    }
}
