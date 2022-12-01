using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatObject_ObjectType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatObjectType",
                table: "Chat_ChatObject",
                newName: "ObjectType");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_ChatObject_ChatObjectType",
                table: "Chat_ChatObject",
                newName: "IX_Chat_ChatObject_ObjectType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ObjectType",
                table: "Chat_ChatObject",
                newName: "ChatObjectType");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_ChatObject_ObjectType",
                table: "Chat_ChatObject",
                newName: "IX_Chat_ChatObject_ChatObjectType");
        }
    }
}
