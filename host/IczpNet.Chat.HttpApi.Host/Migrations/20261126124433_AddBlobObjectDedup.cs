using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddBlobObjectDedup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StorageObjectId",
                table: "Chat_Blob",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_BlobObject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageScope = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Container = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StorageName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RefCount = table.Column<long>(type: "bigint", nullable: false),
                    DeletingSince = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_BlobObject", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Blob_StorageObjectId",
                table: "Chat_Blob",
                column: "StorageObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobObject_Status_CreationTime",
                table: "Chat_BlobObject",
                columns: new[] { "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobObject_StorageScope_Container_Sha256_FileSize",
                table: "Chat_BlobObject",
                columns: new[] { "StorageScope", "Container", "Sha256", "FileSize" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Blob_Chat_BlobObject_StorageObjectId",
                table: "Chat_Blob",
                column: "StorageObjectId",
                principalTable: "Chat_BlobObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Blob_Chat_BlobObject_StorageObjectId",
                table: "Chat_Blob");

            migrationBuilder.DropTable(
                name: "Chat_BlobObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Blob_StorageObjectId",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "StorageObjectId",
                table: "Chat_Blob");
        }
    }
}
