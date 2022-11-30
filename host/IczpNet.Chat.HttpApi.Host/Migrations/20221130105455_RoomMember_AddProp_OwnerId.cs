using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class RoomMember_AddProp_OwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_RoomMember",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_OwnerId",
                table: "Chat_RoomMember",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RoomMember_Chat_ChatObject_OwnerId",
                table: "Chat_RoomMember",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RoomMember_Chat_ChatObject_OwnerId",
                table: "Chat_RoomMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_RoomMember_OwnerId",
                table: "Chat_RoomMember");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_RoomMember");
        }
    }
}
