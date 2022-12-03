using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Room_Change_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "ManagerUserIdList",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Chat_Room");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerUserIdList",
                table: "Chat_Room",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
