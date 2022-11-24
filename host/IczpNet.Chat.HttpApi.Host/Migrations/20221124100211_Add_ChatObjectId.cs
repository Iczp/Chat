using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Add_ChatObjectId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_Room",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_Robot",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_Official",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_ChatObjectId",
                table: "Chat_Room",
                column: "ChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Robot_ChatObjectId",
                table: "Chat_Robot",
                column: "ChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Official_ChatObjectId",
                table: "Chat_Official",
                column: "ChatObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Official_Chat_ChatObject_ChatObjectId",
                table: "Chat_Official",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Robot_Chat_ChatObject_ChatObjectId",
                table: "Chat_Robot",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_ChatObjectId",
                table: "Chat_Room",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Official_Chat_ChatObject_ChatObjectId",
                table: "Chat_Official");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Robot_Chat_ChatObject_ChatObjectId",
                table: "Chat_Robot");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_ChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_ChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Robot_ChatObjectId",
                table: "Chat_Robot");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Official_ChatObjectId",
                table: "Chat_Official");

            migrationBuilder.DropColumn(
                name: "ChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "ChatObjectId",
                table: "Chat_Robot");

            migrationBuilder.DropColumn(
                name: "ChatObjectId",
                table: "Chat_Official");
        }
    }
}
