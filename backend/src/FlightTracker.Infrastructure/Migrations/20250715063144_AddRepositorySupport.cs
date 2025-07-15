using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlightTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepositorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FlightQueries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FlightNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AirlineCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AirlineName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginCode = table.Column<string>(type: "CHAR(3)", nullable: true),
                    DestinationCode = table.Column<string>(type: "CHAR(3)", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CabinClass = table.Column<int>(type: "integer", nullable: false),
                    DeepLink = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DestinationCode",
                        column: x => x.DestinationCode,
                        principalTable: "Airports",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_Flights_Airports_OriginCode",
                        column: x => x.OriginCode,
                        principalTable: "Airports",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "FlightSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlightId = table.Column<Guid>(type: "uuid", nullable: true),
                    FlightNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AirlineCode = table.Column<string>(type: "CHAR(2)", maxLength: 3, nullable: false),
                    OriginCode = table.Column<string>(type: "CHAR(3)", maxLength: 3, nullable: false),
                    DestinationCode = table.Column<string>(type: "CHAR(3)", maxLength: 3, nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AircraftType = table.Column<string>(type: "text", nullable: true),
                    SegmentOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSegments_Airlines_AirlineCode",
                        column: x => x.AirlineCode,
                        principalTable: "Airlines",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightSegments_Airports_DestinationCode",
                        column: x => x.DestinationCode,
                        principalTable: "Airports",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightSegments_Airports_OriginCode",
                        column: x => x.OriginCode,
                        principalTable: "Airports",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightSegments_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationCode",
                table: "Flights",
                column: "DestinationCode");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginCode",
                table: "Flights",
                column: "OriginCode");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSegments_AirlineCode",
                table: "FlightSegments",
                column: "AirlineCode");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSegments_DestinationCode",
                table: "FlightSegments",
                column: "DestinationCode");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSegments_FlightId",
                table: "FlightSegments",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSegments_OriginCode",
                table: "FlightSegments",
                column: "OriginCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightSegments");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FlightQueries");
        }
    }
}
