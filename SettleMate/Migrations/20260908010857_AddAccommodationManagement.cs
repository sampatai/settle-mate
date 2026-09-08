using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleMate.Migrations
{
    /// <inheritdoc />
    public partial class AddAccommodationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Listings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Suburb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeeklyRent = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RoomCount = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AvailableFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDestinations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDestinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListingAmenities",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingAmenities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingAmenities_Listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "dbo",
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListingScores",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TravelTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    NearbySupermarketCount = table.Column<int>(type: "int", nullable: false),
                    NearbyTransportCount = table.Column<int>(type: "int", nullable: false),
                    SafetyScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    StudentScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CalculatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingScores_Listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "dbo",
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListingWeeklyCosts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Included = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingWeeklyCosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingWeeklyCosts_Listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "dbo",
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedListings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedListings_Listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "dbo",
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingAmenities_ListingId_Name",
                schema: "dbo",
                table: "ListingAmenities",
                columns: new[] { "ListingId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_IsActive_Status_Suburb_AvailableFrom",
                schema: "dbo",
                table: "Listings",
                columns: new[] { "IsActive", "Status", "Suburb", "AvailableFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_ProviderId",
                schema: "dbo",
                table: "Listings",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingScores_ListingId_DestinationId",
                schema: "dbo",
                table: "ListingScores",
                columns: new[] { "ListingId", "DestinationId" },
                unique: true,
                filter: "[DestinationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ListingWeeklyCosts_ListingId_Type",
                schema: "dbo",
                table: "ListingWeeklyCosts",
                columns: new[] { "ListingId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedListings_ListingId",
                schema: "dbo",
                table: "SavedListings",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedListings_UserId_ListingId",
                schema: "dbo",
                table: "SavedListings",
                columns: new[] { "UserId", "ListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDestinations_UserId",
                schema: "dbo",
                table: "UserDestinations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingAmenities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ListingScores",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ListingWeeklyCosts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SavedListings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserDestinations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Listings",
                schema: "dbo");
        }
    }
}
