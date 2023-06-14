using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_FixIndex_IsDeleted_TenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_TenantId",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted", "TenantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_TenantId",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId" });
        }
    }
}
