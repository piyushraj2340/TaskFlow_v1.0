using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class Remove_TodoList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TodoList_TodoListsId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TodoListsId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TodoListsId",
                table: "Tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TodoListsId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TodoList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoList_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TodoListsId",
                table: "Tasks",
                column: "TodoListsId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_GoalId",
                table: "TodoList",
                column: "GoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TodoList_TodoListsId",
                table: "Tasks",
                column: "TodoListsId",
                principalTable: "TodoList",
                principalColumn: "Id");
        }
    }
}
