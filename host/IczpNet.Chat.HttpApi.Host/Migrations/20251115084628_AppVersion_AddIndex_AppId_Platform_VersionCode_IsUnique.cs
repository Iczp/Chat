using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AppVersion_AddIndex_AppId_Platform_VersionCode_IsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_AppVersion_AppId_Platform_VersionCode",
                table: "Chat_AppVersion",
                columns: new[] { "AppId", "Platform", "VersionCode" },
                unique: true,
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AppVersion_VersionCode",
                table: "Chat_AppVersion",
                column: "VersionCode",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_AppVersion_AppId_Platform_VersionCode",
                table: "Chat_AppVersion");

            migrationBuilder.DropIndex(
                name: "IX_Chat_AppVersion_VersionCode",
                table: "Chat_AppVersion");
        }
    }
}
