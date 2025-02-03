using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnUpdatedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateOn",
                table: "Tasks",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdateOn",
                table: "Goals",
                newName: "UpdatedOn");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Goals");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Tasks",
                newName: "UpdateOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Goals",
                newName: "UpdateOn");
        }
    }
}
