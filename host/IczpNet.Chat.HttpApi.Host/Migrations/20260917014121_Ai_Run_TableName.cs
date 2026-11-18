using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Ai_Run_TableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatAiRuns",
                table: "ChatAiRuns");

            migrationBuilder.RenameTable(
                name: "ChatAiRuns",
                newName: "Chat_AiRun");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAiRuns_Status_NextAttemptAt",
                table: "Chat_AiRun",
                newName: "IX_Chat_AiRun_Status_NextAttemptAt");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAiRuns_Status_LeaseUntilTime",
                table: "Chat_AiRun",
                newName: "IX_Chat_AiRun_Status_LeaseUntilTime");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAiRuns_SourceMessageId",
                table: "Chat_AiRun",
                newName: "IX_Chat_AiRun_SourceMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAiRuns_SessionId_Status_CreationTime",
                table: "Chat_AiRun",
                newName: "IX_Chat_AiRun_SessionId_Status_CreationTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_AiRun",
                table: "Chat_AiRun",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_AiRun",
                table: "Chat_AiRun");

            migrationBuilder.RenameTable(
                name: "Chat_AiRun",
                newName: "ChatAiRuns");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_AiRun_Status_NextAttemptAt",
                table: "ChatAiRuns",
                newName: "IX_ChatAiRuns_Status_NextAttemptAt");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_AiRun_Status_LeaseUntilTime",
                table: "ChatAiRuns",
                newName: "IX_ChatAiRuns_Status_LeaseUntilTime");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_AiRun_SourceMessageId",
                table: "ChatAiRuns",
                newName: "IX_ChatAiRuns_SourceMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_AiRun_SessionId_Status_CreationTime",
                table: "ChatAiRuns",
                newName: "IX_ChatAiRuns_SessionId_Status_CreationTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatAiRuns",
                table: "ChatAiRuns",
                column: "Id");
        }
    }
}
