using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class CreateGoalTaskRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "GoalTasks",
                columns: table => new
                {
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalTasks", x => new { x.GoalId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_GoalTasks_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalTasks_TaskId",
                table: "GoalTasks",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalTasks");

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
    }
}
