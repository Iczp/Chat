using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Room_Change : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_OwnerChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Chat_RoomMember");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Chat_RoomMember");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Chat_RoomMember");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Chat_RoomMember");

            migrationBuilder.RenameColumn(
                name: "InviterUserId",
                table: "Chat_RoomMember",
                newName: "InviterId");

            migrationBuilder.RenameColumn(
                name: "OwnerChatObjectId",
                table: "Chat_Room",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Room_OwnerChatObjectId",
                table: "Chat_Room",
                newName: "IX_Chat_Room_OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_InviterId",
                table: "Chat_RoomMember",
                column: "InviterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_OwnerId",
                table: "Chat_Room",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RoomMember_Chat_ChatObject_InviterId",
                table: "Chat_RoomMember",
                column: "InviterId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_OwnerId",
                table: "Chat_Room");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RoomMember_Chat_ChatObject_InviterId",
                table: "Chat_RoomMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_RoomMember_InviterId",
                table: "Chat_RoomMember");

            migrationBuilder.RenameColumn(
                name: "InviterId",
                table: "Chat_RoomMember",
                newName: "InviterUserId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_Room",
                newName: "OwnerChatObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Room_OwnerId",
                table: "Chat_Room",
                newName: "IX_Chat_Room_OwnerChatObjectId");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentId",
                table: "Chat_RoomMember",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Chat_RoomMember",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Chat_RoomMember",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PositionId",
                table: "Chat_RoomMember",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_OwnerChatObjectId",
                table: "Chat_Room",
                column: "OwnerChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
