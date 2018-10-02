using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class areanamechange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_House_AreaInspected_AreaId",
                table: "House");

            migrationBuilder.RenameColumn(
                name: "AreaId",
                table: "House",
                newName: "AreaInspectedId");

            migrationBuilder.RenameIndex(
                name: "IX_House_AreaId",
                table: "House",
                newName: "IX_House_AreaInspectedId");

            migrationBuilder.AddForeignKey(
                name: "FK_House_AreaInspected_AreaInspectedId",
                table: "House",
                column: "AreaInspectedId",
                principalTable: "AreaInspected",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_House_AreaInspected_AreaInspectedId",
                table: "House");

            migrationBuilder.RenameColumn(
                name: "AreaInspectedId",
                table: "House",
                newName: "AreaId");

            migrationBuilder.RenameIndex(
                name: "IX_House_AreaInspectedId",
                table: "House",
                newName: "IX_House_AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_House_AreaInspected_AreaId",
                table: "House",
                column: "AreaId",
                principalTable: "AreaInspected",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
