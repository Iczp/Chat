using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ChatObject_AddRelation_Group : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Chat_ChatObject",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject",
                column: "GroupId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Chat_ChatObject");
        }
    }
}
