using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Add_AttachmentLifecycle_And_BlobMessageRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GifBlobId",
                table: "Chat_Message_Template_VideoContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SnapshotBlobId",
                table: "Chat_Message_Template_VideoContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SnapshotThumbnailBlobId",
                table: "Chat_Message_Template_VideoContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalBlobId",
                table: "Chat_Message_Template_ImageContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalUrl",
                table: "Chat_Message_Template_ImageContent",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThumbnailBlobId",
                table: "Chat_Message_Template_ImageContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireTime",
                table: "Chat_Blob",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentBlobId",
                table: "Chat_Blob",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefCount",
                table: "Chat_Blob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Chat_Blob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Chat_Blob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UploadSessionId",
                table: "Chat_Blob",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UploadSessionUnitId",
                table: "Chat_Blob",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UploaderChatObjectId",
                table: "Chat_Blob",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_BlobMessageRef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Chat_BlobMessageRef", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_BlobMessageRef_Chat_Blob_BlobId",
                        column: x => x.BlobId,
                        principalTable: "Chat_Blob",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_BlobMessageRef_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Blob_ParentBlobId",
                table: "Chat_Blob",
                column: "ParentBlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Blob_Status_ExpireTime",
                table: "Chat_Blob",
                columns: new[] { "Status", "ExpireTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Blob_UploaderChatObjectId",
                table: "Chat_Blob",
                column: "UploaderChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobMessageRef_BlobId_MessageId",
                table: "Chat_BlobMessageRef",
                columns: new[] { "BlobId", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobMessageRef_MessageId",
                table: "Chat_BlobMessageRef",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobMessageRef_SessionId",
                table: "Chat_BlobMessageRef",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_BlobMessageRef");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Blob_ParentBlobId",
                table: "Chat_Blob");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Blob_Status_ExpireTime",
                table: "Chat_Blob");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Blob_UploaderChatObjectId",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "GifBlobId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "SnapshotBlobId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "SnapshotThumbnailBlobId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "OriginalBlobId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "OriginalUrl",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "ThumbnailBlobId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "ExpireTime",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "ParentBlobId",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "RefCount",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "UploadSessionId",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "UploadSessionUnitId",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "UploaderChatObjectId",
                table: "Chat_Blob");
        }
    }
}
