using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class Add_Todo_CalculateProductivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculateProductivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalculateDateFor = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalTasks = table.Column<int>(type: "int", nullable: false),
                    TotalCompletedTask = table.Column<int>(type: "int", nullable: false),
                    TotalMissedTask = table.Column<int>(type: "int", nullable: false),
                    ProductivityForDay = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculateProductivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Todo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaskIdId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Todo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Todo_Tasks_TaskIdId",
                        column: x => x.TaskIdId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculateProductivities_CalculateDateFor",
                table: "CalculateProductivities",
                column: "CalculateDateFor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Todo_TaskIdId",
                table: "Todo",
                column: "TaskIdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculateProductivities");

            migrationBuilder.DropTable(
                name: "Todo");
        }
    }
}
