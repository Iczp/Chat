using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_FixIndex_SenderId_ReceiverId_IsPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SenderId_ReceiverId",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderId",
                table: "Chat_Message",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SenderId",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message",
                columns: new[] { "IsPrivate", "SenderId", "ReceiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderId_ReceiverId",
                table: "Chat_Message",
                columns: new[] { "SenderId", "ReceiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted" });
        }
    }
}
