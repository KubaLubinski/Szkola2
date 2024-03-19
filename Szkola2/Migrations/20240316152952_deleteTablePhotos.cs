using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szkola2.Migrations
{
    public partial class deleteTablePhotos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Photos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<int>(type: "int", nullable: true),
                    TextId = table.Column<int>(type: "int", nullable: true),
                    Graphic = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    HistoryId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.PhotoId);
                    table.ForeignKey(
                        name: "FK_Photos_History_Id",
                        column: x => x.Id,
                        principalTable: "History",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Photos_Texts_TextId",
                        column: x => x.TextId,
                        principalTable: "Texts",
                        principalColumn: "TextId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_Id",
                table: "Photos",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_TextId",
                table: "Photos",
                column: "TextId");
        }
    }
}
