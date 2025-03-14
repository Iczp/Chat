using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_Add_ShortId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortId",
                table: "Chat_Message",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "ShortId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ShortId",
                table: "Chat_Message",
                column: "ShortId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ShortId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ShortId",
                table: "Chat_Message");
        }
    }
}
