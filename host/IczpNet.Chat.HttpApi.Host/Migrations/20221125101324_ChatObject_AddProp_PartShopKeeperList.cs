using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatObject_AddProp_PartShopKeeperList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_ShopKeeper",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopKeeper_ChatObjectId",
                table: "Chat_ShopKeeper",
                column: "ChatObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopKeeper",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopKeeper");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ShopKeeper_ChatObjectId",
                table: "Chat_ShopKeeper");

            migrationBuilder.DropColumn(
                name: "ChatObjectId",
                table: "Chat_ShopKeeper");
        }
    }
}
