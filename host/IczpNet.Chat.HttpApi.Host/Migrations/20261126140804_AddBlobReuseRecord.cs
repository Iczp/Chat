using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddBlobReuseRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_BlobReuseRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourcePrimaryBlobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientUploadId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FileNameDigest = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PrimaryBlobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_BlobReuseRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobReuseRecord_ExpireTime",
                table: "Chat_BlobReuseRecord",
                column: "ExpireTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobReuseRecord_UserId_SessionUnitId_SourcePrimaryBlobId_ClientUploadId",
                table: "Chat_BlobReuseRecord",
                columns: new[] { "UserId", "SessionUnitId", "SourcePrimaryBlobId", "ClientUploadId" },
                unique: true,
                filter: "IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_BlobReuseRecord");
        }
    }
}
