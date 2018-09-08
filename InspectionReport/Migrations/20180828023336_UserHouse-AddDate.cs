using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class UserHouseAddDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionDate",
                table: "House",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InspectionDate",
                table: "House");
        }
    }
}
