using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageStat_Index_MessageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_MessageType",
                table: "Chat_MessageStat",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId",
                table: "Chat_MessageStat",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_MessageType",
                table: "Chat_MessageStat");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_SessionId",
                table: "Chat_MessageStat");
        }
    }
}
