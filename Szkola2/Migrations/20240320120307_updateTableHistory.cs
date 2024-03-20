using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szkola2.Migrations
{
    public partial class updateTableHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "History");

            migrationBuilder.AddColumn<string>(
                name: "TextName",
                table: "History",
                type: "nvarchar(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextName",
                table: "History");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "History",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
