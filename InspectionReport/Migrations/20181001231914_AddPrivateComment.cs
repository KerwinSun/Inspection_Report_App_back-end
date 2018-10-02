using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class AddPrivateComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "other",
                table: "AreaInspected",
                newName: "Other");

            migrationBuilder.AddColumn<string>(
                name: "PrivateComments",
                table: "House",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateComments",
                table: "House");

            migrationBuilder.RenameColumn(
                name: "Other",
                table: "AreaInspected",
                newName: "other");
        }
    }
}
