using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class MultiLevelGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Goals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_ParentId",
                table: "Goals",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Goals_ParentId",
                table: "Goals",
                column: "ParentId",
                principalTable: "Goals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Goals_ParentId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_ParentId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Goals");
        }
    }
}
