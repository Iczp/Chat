using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class RedEnvelopeContent_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorUserId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                newName: "CreatorChatObjectId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpireTime",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorChatObjectId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                newName: "CreatorUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpireTime",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
