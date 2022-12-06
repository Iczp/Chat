using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Enum_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoomRole",
                table: "Chat_RoomMember",
                newName: "RoomRoleType");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_RoomMember_RoomRole",
                table: "Chat_RoomMember",
                newName: "IX_Chat_RoomMember_RoomRoleType");

            migrationBuilder.AddColumn<string>(
                name: "RobotType",
                table: "Chat_Robot",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Robot_RobotType",
                table: "Chat_Robot",
                column: "RobotType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Robot_RobotType",
                table: "Chat_Robot");

            migrationBuilder.DropColumn(
                name: "RobotType",
                table: "Chat_Robot");

            migrationBuilder.RenameColumn(
                name: "RoomRoleType",
                table: "Chat_RoomMember",
                newName: "RoomRole");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_RoomMember_RoomRoleType",
                table: "Chat_RoomMember",
                newName: "IX_Chat_RoomMember_RoomRole");
        }
    }
}
