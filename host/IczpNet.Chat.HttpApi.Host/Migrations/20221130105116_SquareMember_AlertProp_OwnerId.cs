using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SquareMember_AlertProp_OwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SquareMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_SquareMember");

            migrationBuilder.RenameColumn(
                name: "ChatObjectId",
                table: "Chat_SquareMember",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SquareMember_ChatObjectId",
                table: "Chat_SquareMember",
                newName: "IX_Chat_SquareMember_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SquareMember_Chat_ChatObject_OwnerId",
                table: "Chat_SquareMember",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SquareMember_Chat_ChatObject_OwnerId",
                table: "Chat_SquareMember");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_SquareMember",
                newName: "ChatObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SquareMember_OwnerId",
                table: "Chat_SquareMember",
                newName: "IX_Chat_SquareMember_ChatObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SquareMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_SquareMember",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
