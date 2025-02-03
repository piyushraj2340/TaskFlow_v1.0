using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class TodoProgressAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculateProductivities");

            migrationBuilder.AddColumn<int>(
                name: "TodoProgressId",
                table: "Todo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TodoProgressAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalculateDateFor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTodo = table.Column<int>(type: "int", nullable: false),
                    TotalCompletedTodo = table.Column<int>(type: "int", nullable: false),
                    TotalMissedTodo = table.Column<int>(type: "int", nullable: false),
                    ProductivityForDay = table.Column<double>(type: "float", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoProgressAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoProgressAnalyses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Todo_TodoProgressId",
                table: "Todo",
                column: "TodoProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoProgressAnalyses_CalculateDateFor",
                table: "TodoProgressAnalyses",
                column: "CalculateDateFor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoProgressAnalyses_UserId",
                table: "TodoProgressAnalyses",
                column: "UserId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "TodoProgressAnalyses");

            migrationBuilder.DropIndex(
                name: "IX_Todo_TodoProgressId",
                table: "Todo");

            migrationBuilder.DropColumn(
                name: "TodoProgressId",
                table: "Todo");

            migrationBuilder.CreateTable(
                name: "CalculateProductivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalculateDateFor = table.Column<DateOnly>(type: "date", nullable: false),
                    ProductivityForDay = table.Column<double>(type: "float", nullable: false),
                    TotalCompletedTask = table.Column<int>(type: "int", nullable: false),
                    TotalMissedTask = table.Column<int>(type: "int", nullable: false),
                    TotalTasks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculateProductivities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculateProductivities_CalculateDateFor",
                table: "CalculateProductivities",
                column: "CalculateDateFor",
                unique: true);
        }
    }
}
