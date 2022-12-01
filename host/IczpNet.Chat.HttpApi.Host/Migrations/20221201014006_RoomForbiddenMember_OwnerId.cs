using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class RoomForbiddenMember_OwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerChatObjectId",
                table: "Chat_RoomForbiddenMember",
                newName: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomForbiddenMember_OwnerId",
                table: "Chat_RoomForbiddenMember",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RoomForbiddenMember_Chat_ChatObject_OwnerId",
                table: "Chat_RoomForbiddenMember",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RoomForbiddenMember_Chat_ChatObject_OwnerId",
                table: "Chat_RoomForbiddenMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_RoomForbiddenMember_OwnerId",
                table: "Chat_RoomForbiddenMember");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_RoomForbiddenMember",
                newName: "OwnerChatObjectId");
        }
    }
}
