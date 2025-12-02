using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitMessage_Fix_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_IsDeleted_SessionUnitId_IsOpened",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_IsDeleted_SessionUnitId_IsRead",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_IsDeleted_SessionUnitId_MessageId",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsFavorited",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "SessionUnitId", "IsFavorited" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsFollowing",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "SessionUnitId", "IsFollowing" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsRead",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "SessionUnitId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsFavorited",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsFollowing",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsRead",
                table: "Chat_SessionUnitMessage");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_IsDeleted_SessionUnitId_IsOpened",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "IsDeleted", "SessionUnitId", "IsOpened" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_IsDeleted_SessionUnitId_IsRead",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "IsDeleted", "SessionUnitId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_IsDeleted_SessionUnitId_MessageId",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "IsDeleted", "SessionUnitId", "MessageId" });
        }
    }
}
