using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szkola2.Migrations
{
    /// <inheritdoc />
    public partial class DemoAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoOwnerId",
                table: "Texts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemoOwnerId",
                table: "PastSchoolEvents",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemoOwnerId",
                table: "Documents",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DemoExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Texts_DemoOwnerId",
                table: "Texts",
                column: "DemoOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PastSchoolEvents_DemoOwnerId",
                table: "PastSchoolEvents",
                column: "DemoOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DemoOwnerId",
                table: "Documents",
                column: "DemoOwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Texts_DemoOwnerId",
                table: "Texts");

            migrationBuilder.DropIndex(
                name: "IX_PastSchoolEvents_DemoOwnerId",
                table: "PastSchoolEvents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DemoOwnerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DemoOwnerId",
                table: "Texts");

            migrationBuilder.DropColumn(
                name: "DemoOwnerId",
                table: "PastSchoolEvents");

            migrationBuilder.DropColumn(
                name: "DemoOwnerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DemoExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);
        }
    }
}
