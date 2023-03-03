using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_Id_Desc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Id",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Id",
                table: "Chat_Message",
                column: "Id",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Id",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Id",
                table: "Chat_Message",
                column: "Id");
        }
    }
}
