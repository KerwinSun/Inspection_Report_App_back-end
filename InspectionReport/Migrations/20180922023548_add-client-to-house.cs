using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class addclienttohouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "House",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimateSummary",
                table: "House",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomsSummary",
                table: "House",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SummonedById",
                table: "House",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    HomePhoneNumber = table.Column<string>(nullable: true),
                    MobilePhoneNumber = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_House_SummonedById",
                table: "House",
                column: "SummonedById");

            migrationBuilder.AddForeignKey(
                name: "FK_House_Client_SummonedById",
                table: "House",
                column: "SummonedById",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_House_Client_SummonedById",
                table: "House");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropIndex(
                name: "IX_House_SummonedById",
                table: "House");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "House");

            migrationBuilder.DropColumn(
                name: "EstimateSummary",
                table: "House");

            migrationBuilder.DropColumn(
                name: "RoomsSummary",
                table: "House");

            migrationBuilder.DropColumn(
                name: "SummonedById",
                table: "House");
        }
    }
}
