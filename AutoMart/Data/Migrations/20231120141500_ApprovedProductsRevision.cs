using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMart.Data.Migrations
{
    public partial class approvedProdus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Vehicle",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Vehicle");
        }
    }
}
