using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistTaskMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                schema: "auth",
                table: "ChecklistTemplates",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeSensitive",
                schema: "auth",
                table: "ChecklistTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                schema: "auth",
                table: "ChecklistTasks",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                schema: "auth",
                table: "ChecklistTemplates");

            migrationBuilder.DropColumn(
                name: "IsTimeSensitive",
                schema: "auth",
                table: "ChecklistTemplates");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "auth",
                table: "ChecklistTasks");
        }
    }
}
