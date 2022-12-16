using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_FixProp_SessionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionIdValue",
                table: "Chat_Message");

            migrationBuilder.RenameColumn(
                name: "SessionIdValue",
                table: "Chat_Message",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_SessionIdValue",
                table: "Chat_Message",
                newName: "IX_Chat_Message_SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Chat_Message",
                newName: "SessionIdValue");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_SessionId",
                table: "Chat_Message",
                newName: "IX_Chat_Message_SessionIdValue");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionIdValue",
                table: "Chat_Message",
                column: "SessionIdValue",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }
    }
}
