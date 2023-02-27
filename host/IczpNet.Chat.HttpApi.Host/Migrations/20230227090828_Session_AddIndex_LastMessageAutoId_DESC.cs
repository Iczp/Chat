using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Session_AddIndex_LastMessageAutoId_DESC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session",
                column: "LastMessageAutoId",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session",
                column: "LastMessageAutoId");
        }
    }
}
