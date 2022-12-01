using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ShopWaiter_IOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopWaiter");

            migrationBuilder.RenameColumn(
                name: "ChatObjectId",
                table: "Chat_ShopWaiter",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_ShopWaiter_ChatObjectId",
                table: "Chat_ShopWaiter",
                newName: "IX_Chat_ShopWaiter_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                table: "Chat_ShopWaiter",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                table: "Chat_ShopWaiter");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_ShopWaiter",
                newName: "ChatObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_ShopWaiter_OwnerId",
                table: "Chat_ShopWaiter",
                newName: "IX_Chat_ShopWaiter_ChatObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopWaiter",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
