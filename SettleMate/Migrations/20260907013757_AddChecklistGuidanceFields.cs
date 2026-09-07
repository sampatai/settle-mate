using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistGuidanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUrl",
                schema: "auth",
                table: "ChecklistTemplates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityNotes",
                schema: "auth",
                table: "ChecklistTemplates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                schema: "auth",
                table: "ChecklistTemplates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentsJson",
                schema: "auth",
                table: "ChecklistTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationUrl",
                schema: "auth",
                table: "ChecklistTemplates");

            migrationBuilder.DropColumn(
                name: "EligibilityNotes",
                schema: "auth",
                table: "ChecklistTemplates");

            migrationBuilder.DropColumn(
                name: "Provider",
                schema: "auth",
                table: "ChecklistTemplates");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentsJson",
                schema: "auth",
                table: "ChecklistTemplates");
        }
    }
}
