using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Session_AlertIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId_Sorting",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit",
                column: "LastMessageAutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit",
                column: "Sorting");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId_Sorting",
                table: "Chat_SessionUnit",
                columns: new[] { "LastMessageAutoId", "Sorting" });
        }
    }
}
