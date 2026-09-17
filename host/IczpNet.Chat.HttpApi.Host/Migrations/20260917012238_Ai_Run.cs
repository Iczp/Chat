using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Ai_Run : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatAiRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceMessageId = table.Column<long>(type: "bigint", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaseOwner = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastHeartbeatAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OutputMessageId = table.Column<long>(type: "bigint", nullable: true),
                    LastErrorCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatAiRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatAiRuns_SessionId_Status_CreationTime",
                table: "ChatAiRuns",
                columns: new[] { "SessionId", "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatAiRuns_SourceMessageId",
                table: "ChatAiRuns",
                column: "SourceMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatAiRuns_Status_LeaseUntil",
                table: "ChatAiRuns",
                columns: new[] { "Status", "LeaseUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatAiRuns_Status_NextAttemptAt",
                table: "ChatAiRuns",
                columns: new[] { "Status", "NextAttemptAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatAiRuns");
        }
    }
}
