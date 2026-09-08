using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations
{
    /// <inheritdoc />
    public partial class MoveOnboardingTablesToDbo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_roles_RoleId",
                schema: "auth",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_roles_RoleId",
                schema: "auth",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                schema: "auth",
                table: "roles");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "roles",
                schema: "auth",
                newName: "Roles",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "VisaRules",
                schema: "auth",
                newName: "VisaRules",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserProfiles",
                schema: "auth",
                newName: "UserProfiles",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Roadmaps",
                schema: "auth",
                newName: "Roadmaps",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RoadmapItems",
                schema: "auth",
                newName: "RoadmapItems",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ChecklistTemplates",
                schema: "auth",
                newName: "ChecklistTemplates",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ChecklistTasks",
                schema: "auth",
                newName: "ChecklistTasks",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                schema: "auth",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_Roles_RoleId",
                schema: "auth",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalSchema: "auth",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Roles_RoleId",
                schema: "auth",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalSchema: "auth",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_Roles_RoleId",
                schema: "auth",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Roles_RoleId",
                schema: "auth",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                schema: "auth",
                table: "Roles");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "auth",
                newName: "roles",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "VisaRules",
                schema: "dbo",
                newName: "VisaRules",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "UserProfiles",
                schema: "dbo",
                newName: "UserProfiles",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "Roadmaps",
                schema: "dbo",
                newName: "Roadmaps",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "RoadmapItems",
                schema: "dbo",
                newName: "RoadmapItems",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "ChecklistTemplates",
                schema: "dbo",
                newName: "ChecklistTemplates",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "ChecklistTasks",
                schema: "dbo",
                newName: "ChecklistTasks",
                newSchema: "auth");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                schema: "auth",
                table: "roles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_roles_RoleId",
                schema: "auth",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalSchema: "auth",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_roles_RoleId",
                schema: "auth",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalSchema: "auth",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
