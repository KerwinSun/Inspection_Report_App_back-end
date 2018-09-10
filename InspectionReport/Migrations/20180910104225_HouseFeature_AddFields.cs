using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class HouseFeature_AddFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "House",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Feature",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "House");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Feature");
        }
    }
}
