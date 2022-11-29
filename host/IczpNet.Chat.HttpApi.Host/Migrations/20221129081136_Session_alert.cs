using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Session_alert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_ChatObjectId",
                table: "Chat_SessionSetting");

            migrationBuilder.RenameColumn(
                name: "ChatObjectId",
                table: "Chat_SessionSetting",
                newName: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_SessionSetting",
                newName: "ChatObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_ChatObjectId",
                table: "Chat_SessionSetting",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
