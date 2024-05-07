using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Connection_AddIndex_CreationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_ServerHost_CreationTime",
                table: "Chat_ServerHost",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Connection_CreationTime",
                table: "Chat_Connection",
                column: "CreationTime",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_ServerHost_CreationTime",
                table: "Chat_ServerHost");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Connection_CreationTime",
                table: "Chat_Connection");
        }
    }
}
