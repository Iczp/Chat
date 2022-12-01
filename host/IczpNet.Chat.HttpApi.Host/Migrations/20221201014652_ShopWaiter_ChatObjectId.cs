using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ShopWaiter_ChatObjectId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopWaiter");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_ShopWaiter",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopWaiter",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopWaiter");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_ShopWaiter",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                table: "Chat_ShopWaiter",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
