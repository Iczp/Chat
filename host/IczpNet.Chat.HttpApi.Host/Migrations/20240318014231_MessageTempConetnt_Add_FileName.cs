using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageTempConetnt_Add_FileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Chat_Message_Template_VideoContent",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Chat_Message_Template_SoundContent",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Chat_Message_Template_ImageContent",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Chat_Message_Template_ImageContent");
        }
    }
}
