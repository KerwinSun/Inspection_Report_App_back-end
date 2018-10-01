using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class ChangeNameOfUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_AspNetUsers_AppLoginUserId",
                table: "AppUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_HouseUser_AppUsers_UserId",
                table: "HouseUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers");

            migrationBuilder.RenameTable(
                name: "AppUsers",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_AppUsers_AppLoginUserId",
                table: "User",
                newName: "IX_User_AppLoginUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HouseUser_User_UserId",
                table: "HouseUser",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_AspNetUsers_AppLoginUserId",
                table: "User",
                column: "AppLoginUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseUser_User_UserId",
                table: "HouseUser");

            migrationBuilder.DropForeignKey(
                name: "FK_User_AspNetUsers_AppLoginUserId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "AppUsers");

            migrationBuilder.RenameIndex(
                name: "IX_User_AppLoginUserId",
                table: "AppUsers",
                newName: "IX_AppUsers_AppLoginUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_AspNetUsers_AppLoginUserId",
                table: "AppUsers",
                column: "AppLoginUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HouseUser_AppUsers_UserId",
                table: "HouseUser",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
