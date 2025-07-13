using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:timescaledb", ",,");

            migrationBuilder.CreateTable(
                name: "Airlines",
                columns: table => new
                {
                    Code = table.Column<string>(type: "CHAR(2)", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Code = table.Column<string>(type: "CHAR(3)", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Timezone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "FlightQueries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OriginCode = table.Column<string>(type: "CHAR(3)", nullable: false),
                    DestinationCode = table.Column<string>(type: "CHAR(3)", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    SearchCount = table.Column<int>(type: "integer", nullable: false),
                    LastSearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightQueries_Airports_DestinationCode",
                        column: x => x.DestinationCode,
                        principalTable: "Airports",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightQueries_Airports_OriginCode",
                        column: x => x.OriginCode,
                        principalTable: "Airports",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceSnapshots",
                columns: table => new
                {
                    QueryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AirlineCode = table.Column<string>(type: "CHAR(2)", nullable: false),
                    Cabin = table.Column<int>(type: "integer", nullable: false),
                    CollectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DeepLink = table.Column<string>(type: "text", nullable: true),
                    FlightNumber = table.Column<string>(type: "text", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Stops = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSnapshots", x => new { x.QueryId, x.AirlineCode, x.Cabin, x.CollectedAt });
                    table.ForeignKey(
                        name: "FK_PriceSnapshots_Airlines_AirlineCode",
                        column: x => x.AirlineCode,
                        principalTable: "Airlines",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceSnapshots_FlightQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "FlightQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightQueries_DestinationCode",
                table: "FlightQueries",
                column: "DestinationCode");

            migrationBuilder.CreateIndex(
                name: "IX_FlightQueries_OriginCode",
                table: "FlightQueries",
                column: "OriginCode");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_AirlineCode",
                table: "PriceSnapshots",
                column: "AirlineCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceSnapshots");

            migrationBuilder.DropTable(
                name: "Airlines");

            migrationBuilder.DropTable(
                name: "FlightQueries");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
