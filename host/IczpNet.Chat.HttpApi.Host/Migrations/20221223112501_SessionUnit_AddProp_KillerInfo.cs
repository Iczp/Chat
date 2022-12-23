using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddProp_KillerInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KillTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KillType",
                table: "Chat_SessionUnit",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KillerId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_KillerId",
                table: "Chat_SessionUnit",
                column: "KillerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_KillType",
                table: "Chat_SessionUnit",
                column: "KillType");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_KillerId",
                table: "Chat_SessionUnit",
                column: "KillerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_KillerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_KillerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_KillType",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillType",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillerId",
                table: "Chat_SessionUnit");
        }
    }
}
