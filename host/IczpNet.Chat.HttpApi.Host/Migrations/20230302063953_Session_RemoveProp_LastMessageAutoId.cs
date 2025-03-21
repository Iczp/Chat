﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Session_RemoveProp_LastMessageAutoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "LastMessageAutoId",
                table: "Chat_Session");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session",
                column: "LastMessageId",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session");

            migrationBuilder.AddColumn<long>(
                name: "LastMessageAutoId",
                table: "Chat_Session",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session",
                column: "LastMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session",
                column: "LastMessageId");
        }
    }
}
