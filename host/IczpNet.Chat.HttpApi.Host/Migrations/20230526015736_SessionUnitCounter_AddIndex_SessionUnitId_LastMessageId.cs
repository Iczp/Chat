﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitCounter_AddIndex_SessionUnitId_LastMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitCounter_SessionUnitId_LastMessageId",
                table: "Chat_SessionUnitCounter",
                columns: new[] { "SessionUnitId", "LastMessageId" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitCounter_SessionUnitId_LastMessageId",
                table: "Chat_SessionUnitCounter");
        }
    }
}
