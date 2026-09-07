using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations;

public partial class AddOnboardingRoadmap : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChecklistTemplates",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                WeekNumber = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                VisaSubclass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CareerGoal = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ChecklistTemplates", x => x.Id));

        migrationBuilder.CreateTable(
            name: "UserProfiles",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                VisaSubclass = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                State = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                UniversityOrEmployer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ArrivalDate = table.Column<DateOnly>(type: "date", nullable: false),
                BudgetRange = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CareerGoal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_UserProfiles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "VisaRules",
            schema: "auth",
            columns: table => new
            {
                VisaSubclass = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                WorkHourLimitPerFortnight = table.Column<int>(type: "int", nullable: true),
                TfnEligible = table.Column<bool>(type: "bit", nullable: false),
                RequiredDocumentsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_VisaRules", x => x.VisaSubclass));

        migrationBuilder.CreateTable(
            name: "Roadmaps",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Roadmaps", x => x.Id));

        migrationBuilder.CreateTable(
            name: "RoadmapItems",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoadmapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WeekNumber = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                LinkedChecklistTaskId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Completed = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoadmapItems", x => x.Id);
                table.ForeignKey("FK_RoadmapItems_Roadmaps_RoadmapId", x => x.RoadmapId,
                    "Roadmaps", "Id", onDelete: ReferentialAction.Cascade, principalSchema: "auth");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChecklistTemplates_Key",
            schema: "auth",
            table: "ChecklistTemplates",
            column: "Key",
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_RoadmapItems_RoadmapId",
            schema: "auth",
            table: "RoadmapItems",
            column: "RoadmapId");
        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_UserId_UserProfileId",
            schema: "auth",
            table: "Roadmaps",
            columns: new[] { "UserId", "UserProfileId" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_UserId_Version",
            schema: "auth",
            table: "UserProfiles",
            columns: new[] { "UserId", "Version" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RoadmapItems", schema: "auth");
        migrationBuilder.DropTable(name: "ChecklistTemplates", schema: "auth");
        migrationBuilder.DropTable(name: "Roadmaps", schema: "auth");
        migrationBuilder.DropTable(name: "UserProfiles", schema: "auth");
        migrationBuilder.DropTable(name: "VisaRules", schema: "auth");
    }
}
