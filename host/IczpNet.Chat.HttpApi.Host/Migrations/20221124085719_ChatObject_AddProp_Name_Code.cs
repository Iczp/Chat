using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatObject_AddProp_Name_Code : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_Robot");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_Official");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Chat_ChatObject",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_ChatObject",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_ChatObject");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_Room",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_Robot",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_Official",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
