using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_FixProp_Channel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageChannel",
                table: "Chat_Message",
                newName: "Channel");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MessageChannel",
                table: "Chat_Message",
                newName: "IX_Chat_Message_Channel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Channel",
                table: "Chat_Message",
                newName: "MessageChannel");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_Channel",
                table: "Chat_Message",
                newName: "IX_Chat_Message_MessageChannel");
        }
    }
}
