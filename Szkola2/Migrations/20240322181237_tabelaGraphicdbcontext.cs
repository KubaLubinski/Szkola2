using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szkola2.Migrations
{
    public partial class tabelaGraphicdbcontext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Graphic_History_HistoryId",
                table: "Graphic");

            migrationBuilder.DropForeignKey(
                name: "FK_Graphic_PastSchoolEvents_EventId",
                table: "Graphic");

            migrationBuilder.DropForeignKey(
                name: "FK_Graphic_Texts_TextId",
                table: "Graphic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Graphic",
                table: "Graphic");

            migrationBuilder.RenameTable(
                name: "Graphic",
                newName: "GraphicGallery");

            migrationBuilder.RenameIndex(
                name: "IX_Graphic_TextId",
                table: "GraphicGallery",
                newName: "IX_GraphicGallery_TextId");

            migrationBuilder.RenameIndex(
                name: "IX_Graphic_HistoryId",
                table: "GraphicGallery",
                newName: "IX_GraphicGallery_HistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Graphic_EventId",
                table: "GraphicGallery",
                newName: "IX_GraphicGallery_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GraphicGallery",
                table: "GraphicGallery",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphicGallery_History_HistoryId",
                table: "GraphicGallery",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphicGallery_PastSchoolEvents_EventId",
                table: "GraphicGallery",
                column: "EventId",
                principalTable: "PastSchoolEvents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphicGallery_Texts_TextId",
                table: "GraphicGallery",
                column: "TextId",
                principalTable: "Texts",
                principalColumn: "TextId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphicGallery_History_HistoryId",
                table: "GraphicGallery");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphicGallery_PastSchoolEvents_EventId",
                table: "GraphicGallery");

            migrationBuilder.DropForeignKey(
                name: "FK_GraphicGallery_Texts_TextId",
                table: "GraphicGallery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GraphicGallery",
                table: "GraphicGallery");

            migrationBuilder.RenameTable(
                name: "GraphicGallery",
                newName: "Graphic");

            migrationBuilder.RenameIndex(
                name: "IX_GraphicGallery_TextId",
                table: "Graphic",
                newName: "IX_Graphic_TextId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphicGallery_HistoryId",
                table: "Graphic",
                newName: "IX_Graphic_HistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphicGallery_EventId",
                table: "Graphic",
                newName: "IX_Graphic_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Graphic",
                table: "Graphic",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Graphic_History_HistoryId",
                table: "Graphic",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Graphic_PastSchoolEvents_EventId",
                table: "Graphic",
                column: "EventId",
                principalTable: "PastSchoolEvents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Graphic_Texts_TextId",
                table: "Graphic",
                column: "TextId",
                principalTable: "Texts",
                principalColumn: "TextId");
        }
    }
}
