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
                    awayOddsAverage = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.resultId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_results_date_homeTeam_awayTeam",
                table: "results",
                columns: new[] { "date", "homeTeam", "awayTeam" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "results");
        }
    }
}
