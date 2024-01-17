using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_AddIndex_LastSendTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendMessageId",
                table: "Chat_SessionUnitSetting",
                column: "LastSendMessageId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendTime",
                table: "Chat_SessionUnitSetting",
                column: "LastSendTime",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_LastSendMessageId",
                table: "Chat_SessionUnitSetting",
                column: "LastSendMessageId");
        }
    }
}
