using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_SenderId_ReceiverId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SenderId",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderId_ReceiverId",
                table: "Chat_Message",
                columns: new[] { "SenderId", "ReceiverId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SenderId_ReceiverId",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderId",
                table: "Chat_Message",
                column: "SenderId");
        }
    }
}
