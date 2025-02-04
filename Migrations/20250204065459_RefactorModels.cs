using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMonitoringApp.Migrations
{
    /// <inheritdoc />
    public partial class RefactorModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_Todo_AspNetUsers_UserId",
                table: "Todo");


            migrationBuilder.DropForeignKey(
                name: "FK_TodoProgressAnalyses_AspNetUsers_UserId",
                table: "TodoProgressAnalyses");

            migrationBuilder.DropIndex(
                name: "IX_Todo_TodoProgressId",
                table: "Todo");

            migrationBuilder.DropColumn(
                name: "TodoProgressId",
                table: "Todo");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TodoProgressAnalyses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "TodoProgressAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Todo",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedOn",
                table: "Todo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndedOn",
                table: "Todo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GoalTasks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_AspNetUsers_UserId",
                table: "Todo",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoProgressAnalyses_AspNetUsers_UserId",
                table: "TodoProgressAnalyses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(
                name: "FK_Todo_AspNetUsers_UserId",
                table: "Todo");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoProgressAnalyses_AspNetUsers_UserId",
                table: "TodoProgressAnalyses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TodoProgressAnalyses");

            migrationBuilder.DropColumn(
                name: "CompletedOn",
                table: "Todo");

            migrationBuilder.DropColumn(
                name: "EndedOn",
                table: "Todo");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TodoProgressAnalyses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Todo",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "TodoProgressId",
                table: "Todo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GoalTasks",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Todo_TodoProgressId",
                table: "Todo",
                column: "TodoProgressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_AspNetUsers_UserId",
                table: "Todo",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_TodoProgressAnalyses_TodoProgressId",
                table: "Todo",
                column: "TodoProgressId",
                principalTable: "TodoProgressAnalyses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoProgressAnalyses_AspNetUsers_UserId",
                table: "TodoProgressAnalyses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
