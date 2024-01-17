using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_AddLastSendMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastSendMessageId",
                table: "Chat_SessionUnitSetting",
                type: "bigint",
                nullable: true,
                comment: "最后发言的消息");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSendTime",
                table: "Chat_SessionUnitSetting",
                type: "datetime2",
                nullable: true,
                comment: "最后发言时间");

            migrationBuilder.AlterColumn<int>(
                name: "PublicBadge",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "消息角标,包含了私有消息 PrivateBadge (未读消息数量)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "最后一条消息Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendMessageId",
                table: "Chat_SessionUnitSetting",
                column: "LastSendMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_Id",
                table: "Chat_Message",
                columns: new[] { "SessionId", "Id" },
                descending: new bool[0]);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_LastSendMessageId",
                table: "Chat_SessionUnitSetting",
                column: "LastSendMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_LastSendMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_Id",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "LastSendMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "LastSendTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.AlterColumn<int>(
                name: "PublicBadge",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "最后一条消息Id",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "消息角标,包含了私有消息 PrivateBadge (未读消息数量)");
        }
    }
}
