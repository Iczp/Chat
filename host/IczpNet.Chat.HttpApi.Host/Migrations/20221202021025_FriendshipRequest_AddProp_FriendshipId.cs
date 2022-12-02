using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class FriendshipRequest_AddProp_FriendshipId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FriendshipId",
                table: "Chat_FriendshipRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FriendshipRequest_FriendshipId",
                table: "Chat_FriendshipRequest",
                column: "FriendshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_FriendshipRequest_Chat_Friendship_FriendshipId",
                table: "Chat_FriendshipRequest",
                column: "FriendshipId",
                principalTable: "Chat_Friendship",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_FriendshipRequest_Chat_Friendship_FriendshipId",
                table: "Chat_FriendshipRequest");

            migrationBuilder.DropIndex(
                name: "IX_Chat_FriendshipRequest_FriendshipId",
                table: "Chat_FriendshipRequest");

            migrationBuilder.DropColumn(
                name: "FriendshipId",
                table: "Chat_FriendshipRequest");
        }
    }
}
