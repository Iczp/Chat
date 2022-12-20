using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Room_AddProp_Session : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InviterId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinWay",
                table: "Chat_SessionUnit",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Chat_Room",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_InviterId",
                table: "Chat_SessionUnit",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_JoinWay",
                table: "Chat_SessionUnit",
                column: "JoinWay");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_SessionId",
                table: "Chat_Room",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_Session_SessionId",
                table: "Chat_Room",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_InviterId",
                table: "Chat_SessionUnit",
                column: "InviterId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_Session_SessionId",
                table: "Chat_Room");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_InviterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_InviterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_JoinWay",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_SessionId",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "InviterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "JoinWay",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Chat_Room");
        }
    }
}
