using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitMessage_Add_OwnerId_SessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Chat_SessionUnitMessage",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Chat_SessionUnitMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_OwnerId",
                table: "Chat_SessionUnitMessage",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionId",
                table: "Chat_SessionUnitMessage",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitMessage_Chat_ChatObject_OwnerId",
                table: "Chat_SessionUnitMessage",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitMessage_Chat_Session_SessionId",
                table: "Chat_SessionUnitMessage",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitMessage_Chat_ChatObject_OwnerId",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitMessage_Chat_Session_SessionId",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_OwnerId",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_SessionId",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Chat_SessionUnitMessage");
        }
    }
}
