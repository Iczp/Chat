using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class VideoContent_Add_SnapshotThumbnailUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Chat_Message_Template_VideoContent",
                newName: "SnapshotUrl");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotThumbnailUrl",
                table: "Chat_Message_Template_VideoContent",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SnapshotThumbnailUrl",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.RenameColumn(
                name: "SnapshotUrl",
                table: "Chat_Message_Template_VideoContent",
                newName: "ImageUrl");
        }
    }
}
