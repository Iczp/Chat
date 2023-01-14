using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_AddIndex_SessionUnitCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_AutoId",
                table: "Chat_Message",
                column: "AutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionUnitCount",
                table: "Chat_Message",
                column: "SessionUnitCount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_AutoId",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionUnitCount",
                table: "Chat_Message");
        }
    }
}
