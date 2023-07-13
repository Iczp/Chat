using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_SenderId_ReceiverId_IsPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message",
                columns: new[] { "IsPrivate", "SenderId", "ReceiverId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_IsPrivate_SenderId_ReceiverId",
                table: "Chat_Message");
        }
    }
}
