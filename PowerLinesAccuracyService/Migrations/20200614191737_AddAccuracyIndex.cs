using Microsoft.EntityFrameworkCore.Migrations;

namespace PowerLinesAccuracyService.Migrations
{
    public partial class AddAccuracyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_accuracy_division",
                table: "accuracy",
                column: "division",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_accuracy_division",
                table: "accuracy");
        }
    }
}
