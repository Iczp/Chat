using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionPermissionGroup_AddIndex_Sorting_FullPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionGroup_FullPath",
                table: "Chat_SessionPermissionGroup",
                column: "FullPath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionGroup_Sorting",
                table: "Chat_SessionPermissionGroup",
                column: "Sorting",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionDefinition_Sorting",
                table: "Chat_SessionPermissionDefinition",
                column: "Sorting",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionPermissionGroup_FullPath",
                table: "Chat_SessionPermissionGroup");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionPermissionGroup_Sorting",
                table: "Chat_SessionPermissionGroup");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionPermissionDefinition_Sorting",
                table: "Chat_SessionPermissionDefinition");
        }
    }
}
