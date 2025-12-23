using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageStat_Fix_DateBucket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_SessionId_Date_MessageType",
                table: "Chat_MessageStat");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Chat_MessageStat",
                newName: "DateBucket");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_DateBucket_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "DateBucket", "MessageType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_SessionId_DateBucket_MessageType",
                table: "Chat_MessageStat");

            migrationBuilder.RenameColumn(
                name: "DateBucket",
                table: "Chat_MessageStat",
                newName: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_Date_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "Date", "MessageType" });
        }
    }
}
