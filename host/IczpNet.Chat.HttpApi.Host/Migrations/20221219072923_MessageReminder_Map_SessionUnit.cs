using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class MessageReminder_Map_SessionUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageReminder_Chat_ChatObject_OwnerId",
                table: "Chat_MessageReminder");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_MessageReminder",
                newName: "SessionUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_MessageReminder_OwnerId",
                table: "Chat_MessageReminder",
                newName: "IX_Chat_MessageReminder_SessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageReminder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageReminder");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_MessageReminder",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_MessageReminder_SessionUnitId",
                table: "Chat_MessageReminder",
                newName: "IX_Chat_MessageReminder_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageReminder_Chat_ChatObject_OwnerId",
                table: "Chat_MessageReminder",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
