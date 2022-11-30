using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Square_AlertProp_CategoryId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Square_Chat_SquareCategory_SquareCategoryId",
                table: "Chat_Square");

            migrationBuilder.RenameColumn(
                name: "SquareCategoryId",
                table: "Chat_Square",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Square_SquareCategoryId",
                table: "Chat_Square",
                newName: "IX_Chat_Square_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Square_Chat_SquareCategory_CategoryId",
                table: "Chat_Square",
                column: "CategoryId",
                principalTable: "Chat_SquareCategory",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Square_Chat_SquareCategory_CategoryId",
                table: "Chat_Square");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Chat_Square",
                newName: "SquareCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Square_CategoryId",
                table: "Chat_Square",
                newName: "IX_Chat_Square_SquareCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Square_Chat_SquareCategory_SquareCategoryId",
                table: "Chat_Square",
                column: "SquareCategoryId",
                principalTable: "Chat_SquareCategory",
                principalColumn: "Id");
        }
    }
}
