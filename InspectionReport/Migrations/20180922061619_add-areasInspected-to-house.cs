using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InspectionReport.Migrations
{
    public partial class addareasInspectedtohouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_House_Client_SummonedById",
                table: "House");

            migrationBuilder.RenameColumn(
                name: "SummonedById",
                table: "House",
                newName: "SummonsedById");

            migrationBuilder.RenameIndex(
                name: "IX_House_SummonedById",
                table: "House",
                newName: "IX_House_SummonsedById");

            migrationBuilder.AddColumn<long>(
                name: "AreaId",
                table: "House",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AreaInspected",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Site = table.Column<bool>(nullable: false),
                    Subfloor = table.Column<bool>(nullable: false),
                    Exterior = table.Column<bool>(nullable: false),
                    RoofExterior = table.Column<bool>(nullable: false),
                    RoofSpace = table.Column<bool>(nullable: false),
                    Services = table.Column<bool>(nullable: false),
                    other = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaInspected", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_House_AreaId",
                table: "House",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_House_AreaInspected_AreaId",
                table: "House",
                column: "AreaId",
                principalTable: "AreaInspected",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_House_Client_SummonsedById",
                table: "House",
                column: "SummonsedById",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_House_AreaInspected_AreaId",
                table: "House");

            migrationBuilder.DropForeignKey(
                name: "FK_House_Client_SummonsedById",
                table: "House");

            migrationBuilder.DropTable(
                name: "AreaInspected");

            migrationBuilder.DropIndex(
                name: "IX_House_AreaId",
                table: "House");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "House");

            migrationBuilder.RenameColumn(
                name: "SummonsedById",
                table: "House",
                newName: "SummonedById");

            migrationBuilder.RenameIndex(
                name: "IX_House_SummonsedById",
                table: "House",
                newName: "IX_House_SummonedById");

            migrationBuilder.AddForeignKey(
                name: "FK_House_Client_SummonedById",
                table: "House",
                column: "SummonedById",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
