using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTasksModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Goals_GoalId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_GoalId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "GoalStatus",
                table: "Tasks",
                newName: "TaskStatus");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Repeat",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RepeatWeekList",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalsTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Repeat",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RepeatWeekList",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "TaskStatus",
                table: "Tasks",
                newName: "GoalStatus");

            migrationBuilder.AddColumn<int>(
                name: "GoalId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_GoalId",
                table: "Tasks",
                column: "GoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Goals_GoalId",
                table: "Tasks",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id");
        }
    }
}
