using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class IOwner_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                table: "Chat_ShopKeeper");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                table: "Chat_ShopWaiter");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ShopKeeper_ShopKeeperId",
                table: "Chat_ShopWaiter");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShopKeeperId",
                table: "Chat_ShopWaiter",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Chat_ShopWaiter",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Chat_ShopKeeper",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                table: "Chat_ShopKeeper",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                table: "Chat_ShopWaiter",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ShopKeeper_ShopKeeperId",
                table: "Chat_ShopWaiter",
                column: "ShopKeeperId",
                principalTable: "Chat_ShopKeeper",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                table: "Chat_ShopKeeper");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                table: "Chat_ShopWaiter");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ShopKeeper_ShopKeeperId",
                table: "Chat_ShopWaiter");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShopKeeperId",
                table: "Chat_ShopWaiter",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Chat_ShopWaiter",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Chat_ShopKeeper",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                table: "Chat_ShopKeeper",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                table: "Chat_ShopWaiter",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ShopWaiter_Chat_ShopKeeper_ShopKeeperId",
                table: "Chat_ShopWaiter",
                column: "ShopKeeperId",
                principalTable: "Chat_ShopKeeper",
                principalColumn: "Id");
        }
    }
}
