using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szkola2.Migrations
{
    public partial class tabelaGraphic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Graphic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextId = table.Column<int>(type: "int", nullable: true),
                    HistoryId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Graphic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Graphic_History_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "History",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Graphic_PastSchoolEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "PastSchoolEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Graphic_Texts_TextId",
                        column: x => x.TextId,
                        principalTable: "Texts",
                        principalColumn: "TextId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Graphic_EventId",
                table: "Graphic",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Graphic_HistoryId",
                table: "Graphic",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Graphic_TextId",
                table: "Graphic",
                column: "TextId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Graphic");
        }
    }
}
