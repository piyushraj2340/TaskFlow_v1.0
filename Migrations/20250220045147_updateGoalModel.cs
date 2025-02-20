using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class updateGoalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                table: "Goals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarted",
                table: "Goals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Goals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedOn",
                table: "Goals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsScheduled",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "IsStarted",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "StartedOn",
                table: "Goals");
        }
    }
}
