using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_AddIndex_Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_CreationTime_SessionUnitId",
                table: "Chat_SessionUnitSetting",
                columns: new[] { "CreationTime", "SessionUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_Rename",
                table: "Chat_SessionUnitSetting",
                column: "Rename");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_RenameSpelling",
                table: "Chat_SessionUnitSetting",
                column: "RenameSpelling");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_RenameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                column: "RenameSpellingAbbreviation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_CreationTime_SessionUnitId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_Rename",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_RenameSpelling",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_RenameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting");
        }
    }
}
