using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class EntryValue_AddIndex_EntryNameId_Value_Asc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_EntryValue_EntryNameId",
                table: "Chat_EntryValue");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryValue_EntryNameId_Value",
                table: "Chat_EntryValue",
                columns: new[] { "EntryNameId", "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_EntryValue_EntryNameId_Value",
                table: "Chat_EntryValue");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryValue_EntryNameId",
                table: "Chat_EntryValue",
                column: "EntryNameId");
        }
    }
}
