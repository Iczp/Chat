using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Session_AddPop_Rename_IsImmersed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<bool>(
                name: "IsImmersed",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Rename",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImmersed",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "Rename",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_SessionUnit",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
