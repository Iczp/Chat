using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddIndex_LastMessageAutoId_DESC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit",
                column: "LastMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit",
                column: "Sorting",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit",
                column: "LastMessageAutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageAutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit",
                column: "Sorting");
        }
    }
}
