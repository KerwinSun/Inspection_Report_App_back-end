using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class AppUserRel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AppUsers_UserId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "AppLoginUserId",
                table: "AppUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_AppLoginUserId",
                table: "AppUsers",
                column: "AppLoginUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_AspNetUsers_AppLoginUserId",
                table: "AppUsers",
                column: "AppLoginUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_AspNetUsers_AppLoginUserId",
                table: "AppUsers");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_AppLoginUserId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "AppLoginUserId",
                table: "AppUsers");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AppUsers_UserId",
                table: "AspNetUsers",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
