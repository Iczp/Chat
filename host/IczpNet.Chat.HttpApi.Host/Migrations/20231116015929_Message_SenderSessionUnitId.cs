using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_SenderSessionUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Message");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_Message",
                newName: "SenderSessionUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_SessionUnitId",
                table: "Chat_Message",
                newName: "IX_Chat_Message_SenderSessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_SenderSessionUnitId",
                table: "Chat_Message",
                column: "SenderSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_SenderSessionUnitId",
                table: "Chat_Message");

            migrationBuilder.RenameColumn(
                name: "SenderSessionUnitId",
                table: "Chat_Message",
                newName: "SessionUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_SenderSessionUnitId",
                table: "Chat_Message",
                newName: "IX_Chat_Message_SessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Message",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }
    }
}
