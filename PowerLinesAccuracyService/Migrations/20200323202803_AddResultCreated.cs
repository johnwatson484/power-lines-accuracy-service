using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PowerLinesAccuracyService.Migrations
{
    public partial class AddResultCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "results",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created",
                table: "results");
        }
    }
}
