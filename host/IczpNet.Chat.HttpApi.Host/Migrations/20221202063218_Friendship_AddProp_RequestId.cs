using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Friendship_AddProp_RequestId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "Chat_Friendship",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Friendship_RequestId",
                table: "Chat_Friendship",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Friendship_Chat_FriendshipRequest_RequestId",
                table: "Chat_Friendship",
                column: "RequestId",
                principalTable: "Chat_FriendshipRequest",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Friendship_Chat_FriendshipRequest_RequestId",
                table: "Chat_Friendship");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Friendship_RequestId",
                table: "Chat_Friendship");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Chat_Friendship");

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
    }
}
