using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoGifPropagationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_VideoGifPropagationOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceMessageId = table.Column<long>(type: "bigint", nullable: false),
                    VideoContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GifBlobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GifUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_Chat_VideoGifPropagationOutbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_VideoGifPropagationOutbox_ProcessedTime_NextRetryTime",
                table: "Chat_VideoGifPropagationOutbox",
                columns: new[] { "ProcessedTime", "NextRetryTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_VideoGifPropagationOutbox");
        }
    }
}
