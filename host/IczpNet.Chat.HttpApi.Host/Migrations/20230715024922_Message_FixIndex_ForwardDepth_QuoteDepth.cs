using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_FixIndex_ForwardDepth_QuoteDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ForwardDepth",
                table: "Chat_Message",
                column: "ForwardDepth");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_QuoteDepth",
                table: "Chat_Message",
                column: "QuoteDepth");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted", "CreationTime", "ForwardDepth", "QuoteDepth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ForwardDepth",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_QuoteDepth",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted", "CreationTime" });
        }
    }
}
