using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistIntegrationAndEligibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BlueCardRequiredForChildRelatedWork",
                schema: "auth",
                table: "VisaRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NdisEligible",
                schema: "auth",
                table: "VisaRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ChecklistTaskId",
                schema: "auth",
                table: "RoadmapItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ChecklistTasks",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapItems_ChecklistTaskId",
                schema: "auth",
                table: "RoadmapItems",
                column: "ChecklistTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTasks_UserId_Key",
                schema: "auth",
                table: "ChecklistTasks",
                columns: new[] { "UserId", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RoadmapItems_ChecklistTasks_ChecklistTaskId",
                schema: "auth",
                table: "RoadmapItems",
                column: "ChecklistTaskId",
                principalSchema: "auth",
                principalTable: "ChecklistTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoadmapItems_ChecklistTasks_ChecklistTaskId",
                schema: "auth",
                table: "RoadmapItems");

            migrationBuilder.DropTable(
                name: "ChecklistTasks",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "IX_RoadmapItems_ChecklistTaskId",
                schema: "auth",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "BlueCardRequiredForChildRelatedWork",
                schema: "auth",
                table: "VisaRules");

            migrationBuilder.DropColumn(
                name: "NdisEligible",
                schema: "auth",
                table: "VisaRules");

            migrationBuilder.DropColumn(
                name: "ChecklistTaskId",
                schema: "auth",
                table: "RoadmapItems");
        }
    }
}
