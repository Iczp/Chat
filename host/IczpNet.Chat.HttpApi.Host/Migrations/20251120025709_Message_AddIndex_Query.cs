using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_Query : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderSessionUnitId_ReceiverSessionUnitId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsDeleted_IsPrivate_SenderId_ReceiverId_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsDeleted", "IsPrivate", "SenderId", "ReceiverId", "CreationTime", "ForwardDepth", "QuoteDepth" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsDeleted_IsPrivate_SenderSessionUnitId_ReceiverSessionUnitId_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsDeleted", "IsPrivate", "SenderSessionUnitId", "ReceiverSessionUnitId", "CreationTime", "ForwardDepth", "QuoteDepth" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_Session_Count",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsDeleted", "IsPrivate", "SenderSessionUnitId", "ReceiverSessionUnitId" })
                .Annotation("SqlServer:Include", new[] { "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsDeleted_IsPrivate_SenderId_ReceiverId_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsDeleted_IsPrivate_SenderSessionUnitId_ReceiverSessionUnitId_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Message_Session_Count",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted", "CreationTime", "ForwardDepth", "QuoteDepth" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderSessionUnitId_ReceiverSessionUnitId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderSessionUnitId", "ReceiverSessionUnitId", "IsDeleted", "CreationTime", "ForwardDepth", "QuoteDepth" });
        }
    }
}
