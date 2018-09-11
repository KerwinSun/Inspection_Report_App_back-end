using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class changeNotesToComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Feature",
                newName: "Comments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "Feature",
                newName: "Notes");
        }
    }
}
