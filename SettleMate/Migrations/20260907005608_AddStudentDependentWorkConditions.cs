using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentDependentWorkConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DependentBachelorWorkHourLimitPerFortnight",
                schema: "auth",
                table: "VisaRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DependentPostgraduateWorkHourLimitPerFortnight",
                schema: "auth",
                table: "VisaRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantType",
                schema: "auth",
                table: "UserProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CourseStarted",
                schema: "auth",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StudyLevel",
                schema: "auth",
                table: "UserProfiles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DependentBachelorWorkHourLimitPerFortnight",
                schema: "auth",
                table: "VisaRules");

            migrationBuilder.DropColumn(
                name: "DependentPostgraduateWorkHourLimitPerFortnight",
                schema: "auth",
                table: "VisaRules");

            migrationBuilder.DropColumn(
                name: "ApplicantType",
                schema: "auth",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CourseStarted",
                schema: "auth",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "StudyLevel",
                schema: "auth",
                table: "UserProfiles");
        }
    }
}
