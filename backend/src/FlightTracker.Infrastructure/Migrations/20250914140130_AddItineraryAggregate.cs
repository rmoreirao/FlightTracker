using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItineraryAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Itineraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TotalPriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItineraryLegs",
                columns: table => new
                {
                    ItineraryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    FlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlightNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AirlineCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OriginCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DestinationCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DepartureUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LegPriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    LegPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CabinClass = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryLegs", x => new { x.ItineraryId, x.Sequence });
                    table.ForeignKey(
                        name: "FK_ItineraryLegs_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryLegs_FlightId",
                table: "ItineraryLegs",
                column: "FlightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItineraryLegs");

            migrationBuilder.DropTable(
                name: "Itineraries");
        }
    }
}
