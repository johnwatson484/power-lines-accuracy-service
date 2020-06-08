using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PowerLinesAccuracyService.Migrations
{
    public partial class AddAccuracy : Migration
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accuracy");
        }
    }
}
