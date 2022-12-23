using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Session_AddProp_LastMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastMessageId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session",
                column: "LastMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_Message_LastMessageId",
                table: "Chat_Session",
                column: "LastMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_Message_LastMessageId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                table: "Chat_Session");
        }
    }
}
