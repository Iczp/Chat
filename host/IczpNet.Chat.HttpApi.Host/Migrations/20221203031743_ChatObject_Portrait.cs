using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatObject_Portrait : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portrait",
                table: "Chat_Room");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "Chat_ChatObject",
                newName: "AppUserId");

            migrationBuilder.AddColumn<string>(
                name: "Portrait",
                table: "Chat_ChatObject",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portrait",
                table: "Chat_ChatObject");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Chat_ChatObject",
                newName: "OwnerUserId");

            migrationBuilder.AddColumn<string>(
                name: "Portrait",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
