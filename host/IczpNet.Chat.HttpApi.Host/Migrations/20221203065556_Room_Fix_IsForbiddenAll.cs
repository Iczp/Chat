using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Room_Fix_IsForbiddenAll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsForbiddenInput",
                table: "Chat_Room",
                newName: "IsForbiddenAll");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsForbiddenAll",
                table: "Chat_Room",
                newName: "IsForbiddenInput");
        }
    }
}
