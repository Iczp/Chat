using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddIndex_Sorting_Desc_LastMessageId_Asc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId" },
                descending: new[] { true, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit");
        }
    }
}
