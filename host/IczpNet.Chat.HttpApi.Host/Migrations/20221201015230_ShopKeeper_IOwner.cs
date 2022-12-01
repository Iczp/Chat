using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ShopKeeper_IOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_ShopKeeper",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopKeeper_OwnerId",
                table: "Chat_ShopKeeper",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                table: "Chat_ShopKeeper",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                table: "Chat_ShopKeeper");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ShopKeeper_OwnerId",
                table: "Chat_ShopKeeper");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_ShopKeeper");

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
    }
}
