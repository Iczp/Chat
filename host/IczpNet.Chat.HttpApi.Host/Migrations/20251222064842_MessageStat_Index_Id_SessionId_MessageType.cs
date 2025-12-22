using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageStat_Index_Id_SessionId_MessageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_Id_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "Id", "SessionId", "MessageType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_Id_SessionId_MessageType",
                table: "Chat_MessageStat");
        }
    }
}
