using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_addProp_SessionUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionUnitId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionUnitId",
                table: "Chat_Message",
                column: "SessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Message",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionUnitId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "SessionUnitId",
                table: "Chat_Message");
        }
    }
}
