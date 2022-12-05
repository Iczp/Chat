using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Friendship_Fix_DestinationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Friendship_Chat_ChatObject_FriendId",
                table: "Chat_Friendship");

            migrationBuilder.RenameColumn(
                name: "FriendId",
                table: "Chat_Friendship",
                newName: "DestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Friendship_FriendId",
                table: "Chat_Friendship",
                newName: "IX_Chat_Friendship_DestinationId");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Chat_FriendshipRequest",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Friendship_Chat_ChatObject_DestinationId",
                table: "Chat_Friendship",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Friendship_Chat_ChatObject_DestinationId",
                table: "Chat_Friendship");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Chat_FriendshipRequest");

            migrationBuilder.RenameColumn(
                name: "DestinationId",
                table: "Chat_Friendship",
                newName: "FriendId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Friendship_DestinationId",
                table: "Chat_Friendship",
                newName: "IX_Chat_Friendship_FriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Friendship_Chat_ChatObject_FriendId",
                table: "Chat_Friendship",
                column: "FriendId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
