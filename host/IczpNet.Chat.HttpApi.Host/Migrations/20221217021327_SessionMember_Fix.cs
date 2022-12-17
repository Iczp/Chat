using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionMember_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestinationId",
                table: "Chat_SessionMember",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionMember_DestinationId",
                table: "Chat_SessionMember",
                column: "DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionMember_Chat_ChatObject_DestinationId",
                table: "Chat_SessionMember",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionMember_Chat_ChatObject_DestinationId",
                table: "Chat_SessionMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionMember_DestinationId",
                table: "Chat_SessionMember");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_SessionMember");
        }
    }
}
