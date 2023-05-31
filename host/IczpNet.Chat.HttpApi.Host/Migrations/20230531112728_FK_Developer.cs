using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class FK_Developer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
