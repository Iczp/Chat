using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_Add_ReceiverSessionUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SenderSessionUnitId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                comment: "发送人会话单元Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "会话单元Id");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverSessionUnitId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                comment: "接收人会话单元Id(私有消息)");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ReceiverSessionUnitId",
                table: "Chat_Message",
                column: "ReceiverSessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_ReceiverSessionUnitId",
                table: "Chat_Message",
                column: "ReceiverSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_ReceiverSessionUnitId",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ReceiverSessionUnitId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ReceiverSessionUnitId",
                table: "Chat_Message");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderSessionUnitId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                comment: "会话单元Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "发送人会话单元Id");
        }
    }
}
