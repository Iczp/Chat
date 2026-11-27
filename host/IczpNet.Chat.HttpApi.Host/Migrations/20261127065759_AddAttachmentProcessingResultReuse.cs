using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentProcessingResultReuse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_AttachmentProcessingResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageScope = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Container = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceFileSize = table.Column<long>(type: "bigint", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    OutputRole = table.Column<int>(type: "int", nullable: false),
                    RecipeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OutputStorageObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutputMimeType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OutputSuffix = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    OutputFileSize = table.Column<long>(type: "bigint", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LeaseOwner = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LeaseExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    LastVerifiedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_AttachmentProcessingResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_AttachmentProcessingWaiter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessingResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryBlobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutputRole = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_AttachmentProcessingWaiter", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AttachmentProcessingResult_OutputStorageObjectId",
                table: "Chat_AttachmentProcessingResult",
                column: "OutputStorageObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AttachmentProcessingResult_Status_LeaseExpireTime",
                table: "Chat_AttachmentProcessingResult",
                columns: new[] { "Status", "LeaseExpireTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AttachmentProcessingResult_StorageScope_Container_SourceSha256_SourceFileSize_MediaType_OutputRole_RecipeHash",
                table: "Chat_AttachmentProcessingResult",
                columns: new[] { "StorageScope", "Container", "SourceSha256", "SourceFileSize", "MediaType", "OutputRole", "RecipeHash" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AttachmentProcessingWaiter_MessageId_OutputRole",
                table: "Chat_AttachmentProcessingWaiter",
                columns: new[] { "MessageId", "OutputRole" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AttachmentProcessingWaiter_ProcessingResultId_Status",
                table: "Chat_AttachmentProcessingWaiter",
                columns: new[] { "ProcessingResultId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_AttachmentProcessingResult");

            migrationBuilder.DropTable(
                name: "Chat_AttachmentProcessingWaiter");
        }
    }
}
