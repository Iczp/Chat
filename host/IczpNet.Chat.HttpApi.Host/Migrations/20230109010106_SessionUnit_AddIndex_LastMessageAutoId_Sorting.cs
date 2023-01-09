using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddIndex_LastMessageAutoId_Sorting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId_Sorting",
                table: "Chat_SessionUnit",
                columns: new[] { "LastMessageAutoId", "Sorting" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId_Sorting",
                table: "Chat_SessionUnit");
        }
    }
}
