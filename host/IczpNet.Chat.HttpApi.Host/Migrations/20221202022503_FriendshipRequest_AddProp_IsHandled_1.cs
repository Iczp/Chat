using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class FriendshipRequest_AddProp_IsHandled_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ishandled",
                table: "Chat_FriendshipRequest",
                newName: "IsHandled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsHandled",
                table: "Chat_FriendshipRequest",
                newName: "Ishandled");
        }
    }
}
