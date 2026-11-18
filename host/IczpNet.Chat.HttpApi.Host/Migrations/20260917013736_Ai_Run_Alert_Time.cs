using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Ai_Run_Alert_Time : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "ChatAiRuns",
                newName: "StartedTime");

            migrationBuilder.RenameColumn(
                name: "LeaseUntil",
                table: "ChatAiRuns",
                newName: "LeaseUntilTime");

            migrationBuilder.RenameColumn(
                name: "LastHeartbeatAt",
                table: "ChatAiRuns",
                newName: "LastHeartbeatTime");

            migrationBuilder.RenameColumn(
                name: "DeadlineAt",
                table: "ChatAiRuns",
                newName: "DeadlineTime");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "ChatAiRuns",
                newName: "CompletedTime");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAiRuns_Status_LeaseUntil",
                table: "ChatAiRuns",
                newName: "IX_ChatAiRuns_Status_LeaseUntilTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartedTime",
                table: "ChatAiRuns",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "LeaseUntilTime",
                table: "ChatAiRuns",
                newName: "LeaseUntil");

            migrationBuilder.RenameColumn(
                name: "LastHeartbeatTime",
                table: "ChatAiRuns",
                newName: "LastHeartbeatAt");

            migrationBuilder.RenameColumn(
                name: "DeadlineTime",
                table: "ChatAiRuns",
                newName: "DeadlineAt");

            migrationBuilder.RenameColumn(
                name: "CompletedTime",
                table: "ChatAiRuns",
                newName: "CompletedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAiRuns_Status_LeaseUntilTime",
                table: "ChatAiRuns",
                newName: "IX_ChatAiRuns_Status_LeaseUntil");
        }
    }
}
