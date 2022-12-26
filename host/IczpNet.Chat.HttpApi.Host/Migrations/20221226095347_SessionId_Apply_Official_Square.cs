using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionId_Apply_Official_Square : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Chat_Square",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Chat_Official",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Square_SessionId",
                table: "Chat_Square",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Official_SessionId",
                table: "Chat_Official",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Official_Chat_Session_SessionId",
                table: "Chat_Official",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Square_Chat_Session_SessionId",
                table: "Chat_Square",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Official_Chat_Session_SessionId",
                table: "Chat_Official");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Square_Chat_Session_SessionId",
                table: "Chat_Square");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Square_SessionId",
                table: "Chat_Square");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Official_SessionId",
                table: "Chat_Official");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Chat_Square");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Chat_Official");
        }
    }
}
