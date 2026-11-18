using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_Add_ClientMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientMessageId",
                table: "Chat_Message",
                type: "nvarchar(26)",
                maxLength: 26,
                nullable: true,
                comment: "ClientMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ClientMessageId",
                table: "Chat_Message",
                column: "ClientMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ClientMessageId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ClientMessageId",
                table: "Chat_Message");
        }
    }
}
