using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Session_AddProp_Owner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                table: "Chat_Session",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Session");
        }
    }
}
