using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PowerLinesAccuracyService.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accuracy",
                columns: table => new
                {
                    accuracyId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    division = table.Column<string>(nullable: true),
                    matches = table.Column<int>(nullable: false),
                    recommended = table.Column<int>(nullable: false),
                    recommendedAccuracy = table.Column<decimal>(nullable: false),
                    lowerRecommended = table.Column<int>(nullable: false),
                    lowerRecommendedAccuracy = table.Column<decimal>(nullable: false),
                    calculated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accuracy", x => x.accuracyId);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    resultId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    division = table.Column<string>(nullable: true),
                    date = table.Column<DateTime>(nullable: false),
                    homeTeam = table.Column<string>(nullable: true),
                    awayTeam = table.Column<string>(nullable: true),
                    fullTimeHomeGoals = table.Column<int>(nullable: false),
                    fullTimeAwayGoals = table.Column<int>(nullable: false),
                    fullTimeResult = table.Column<string>(nullable: true),
                    halfTimeHomeGoals = table.Column<int>(nullable: false),
                    halfTimeAwayGoals = table.Column<int>(nullable: false),
                    halfTimeResult = table.Column<string>(nullable: true),
                    homeOddsAverage = table.Column<decimal>(nullable: false),
                    drawOddsAverage = table.Column<decimal>(nullable: false),
                    awayOddsAverage = table.Column<decimal>(nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.resultId);
                });

            migrationBuilder.CreateTable(
                name: "MatchOdds",
                columns: table => new
                {
                    matchOddsId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resultId = table.Column<int>(nullable: false),
                    home = table.Column<decimal>(nullable: false),
                    draw = table.Column<decimal>(nullable: false),
                    away = table.Column<decimal>(nullable: false),
                    expectedHomeGoals = table.Column<int>(nullable: false),
                    expectedAwayGoals = table.Column<int>(nullable: false),
                    expectedGoals = table.Column<decimal>(nullable: false),
                    recommended = table.Column<string>(nullable: true),
                    lowerRecommended = table.Column<string>(nullable: true),
                    calculated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchOdds", x => x.matchOddsId);
                    table.ForeignKey(
                        name: "FK_MatchOdds_results_resultId",
                        column: x => x.resultId,
                        principalTable: "results",
                        principalColumn: "resultId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchOdds_resultId",
                table: "MatchOdds",
                column: "resultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_results_date_homeTeam_awayTeam",
                table: "results",
                columns: new[] { "date", "homeTeam", "awayTeam" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accuracy");

            migrationBuilder.DropTable(
                name: "MatchOdds");

            migrationBuilder.DropTable(
                name: "results");
        }
    }
}
