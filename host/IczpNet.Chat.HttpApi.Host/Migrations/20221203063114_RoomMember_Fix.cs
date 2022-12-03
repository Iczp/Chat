using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class RoomMember_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RoomRoleRoomMember_Chat_RoomRole_RoleId",
                table: "Chat_RoomRoleRoomMember");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Chat_RoomRoleRoomMember",
                newName: "RoomRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RoomRoleRoomMember_Chat_RoomRole_RoomRoleId",
                table: "Chat_RoomRoleRoomMember",
                column: "RoomRoleId",
                principalTable: "Chat_RoomRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RoomRoleRoomMember_Chat_RoomRole_RoomRoleId",
                table: "Chat_RoomRoleRoomMember");

            migrationBuilder.RenameColumn(
                name: "RoomRoleId",
                table: "Chat_RoomRoleRoomMember",
                newName: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RoomRoleRoomMember_Chat_RoomRole_RoleId",
                table: "Chat_RoomRoleRoomMember",
                column: "RoleId",
                principalTable: "Chat_RoomRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
