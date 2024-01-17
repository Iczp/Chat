using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_Add_SessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_SessionId",
                table: "Chat_SessionUnitSetting",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Session_SessionId",
                table: "Chat_SessionUnitSetting",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Session_SessionId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_SessionId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Chat_SessionUnitSetting");
        }
    }
}
