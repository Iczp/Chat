using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddIndex_LastMessageAutoId_Sorting_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageAutoId",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageAutoId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageAutoId",
                table: "Chat_SessionUnit");
        }
    }
}
