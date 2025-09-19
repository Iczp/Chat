using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_SenderSessionUnitId_ReceiverSessionUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_CreationTime",
                table: "Chat_Session",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderSessionUnitId_ReceiverSessionUnitId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderSessionUnitId", "ReceiverSessionUnitId", "IsDeleted", "CreationTime", "ForwardDepth", "QuoteDepth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_CreationTime",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderSessionUnitId_ReceiverSessionUnitId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message");
        }
    }
}
