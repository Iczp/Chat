using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Session_FirstMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_ReadedMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.RenameColumn(
                name: "ReadedMessageId",
                table: "Chat_SessionUnitSetting",
                newName: "ReadMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionUnitSetting_ReadedMessageId",
                table: "Chat_SessionUnitSetting",
                newName: "IX_Chat_SessionUnitSetting_ReadMessageId");

            migrationBuilder.AddColumn<long>(
                name: "EndMessageId",
                table: "Chat_SessionUnitSetting",
                type: "bigint",
                nullable: true,
                comment: "显示结束消息Id");

            migrationBuilder.AddColumn<long>(
                name: "PeerReadMessageId",
                table: "Chat_SessionUnitSetting",
                type: "bigint",
                nullable: true,
                comment: "对方已读的消息");

            migrationBuilder.AddColumn<long>(
                name: "StartMessageId",
                table: "Chat_SessionUnitSetting",
                type: "bigint",
                nullable: true,
                comment: "显示开始消息Id");

            migrationBuilder.AddColumn<long>(
                name: "FirstMessageId",
                table: "Chat_Session",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_EndMessageId",
                table: "Chat_SessionUnitSetting",
                column: "EndMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_PeerReadMessageId",
                table: "Chat_SessionUnitSetting",
                column: "PeerReadMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_StartMessageId",
                table: "Chat_SessionUnitSetting",
                column: "StartMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_FirstMessageId",
                table: "Chat_Session",
                column: "FirstMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_Message_FirstMessageId",
                table: "Chat_Session",
                column: "FirstMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_EndMessageId",
                table: "Chat_SessionUnitSetting",
                column: "EndMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_PeerReadMessageId",
                table: "Chat_SessionUnitSetting",
                column: "PeerReadMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_ReadMessageId",
                table: "Chat_SessionUnitSetting",
                column: "ReadMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_StartMessageId",
                table: "Chat_SessionUnitSetting",
                column: "StartMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_Message_FirstMessageId",
                table: "Chat_Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_EndMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_PeerReadMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_ReadMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_StartMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_EndMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_PeerReadMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_StartMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_FirstMessageId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "EndMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "PeerReadMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "StartMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "FirstMessageId",
                table: "Chat_Session");

            migrationBuilder.RenameColumn(
                name: "ReadMessageId",
                table: "Chat_SessionUnitSetting",
                newName: "ReadedMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionUnitSetting_ReadMessageId",
                table: "Chat_SessionUnitSetting",
                newName: "IX_Chat_SessionUnitSetting_ReadedMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitSetting_Chat_Message_ReadedMessageId",
                table: "Chat_SessionUnitSetting",
                column: "ReadedMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }
    }
}
