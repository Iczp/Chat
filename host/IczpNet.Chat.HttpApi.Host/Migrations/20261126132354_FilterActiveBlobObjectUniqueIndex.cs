using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class FilterActiveBlobObjectUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_BlobObject_StorageScope_Container_Sha256_FileSize",
                table: "Chat_BlobObject");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobObject_StorageScope_Container_Sha256_FileSize",
                table: "Chat_BlobObject",
                columns: new[] { "StorageScope", "Container", "Sha256", "FileSize" },
                unique: true,
                filter: "IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_BlobObject_StorageScope_Container_Sha256_FileSize",
                table: "Chat_BlobObject");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_BlobObject_StorageScope_Container_Sha256_FileSize",
                table: "Chat_BlobObject",
                columns: new[] { "StorageScope", "Container", "Sha256", "FileSize" },
                unique: true);
        }
    }
}
