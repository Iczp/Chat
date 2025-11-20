using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_SessionId_IsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsDeleted",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsDeleted",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId",
                table: "Chat_Message",
                column: "SessionId");
        }
    }
}
