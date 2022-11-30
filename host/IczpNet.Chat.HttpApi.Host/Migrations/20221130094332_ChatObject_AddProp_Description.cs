using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatObject_AddProp_Description : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_Room");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_ChatObject",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_ChatObject");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_Room",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
