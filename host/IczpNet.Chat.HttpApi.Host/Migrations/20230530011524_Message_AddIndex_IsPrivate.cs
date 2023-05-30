using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_IsPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_IsPrivate",
                table: "Chat_Message",
                column: "IsPrivate");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MessageType",
                table: "Chat_Message",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_IsPrivate",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_MessageType",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message");
        }
    }
}
