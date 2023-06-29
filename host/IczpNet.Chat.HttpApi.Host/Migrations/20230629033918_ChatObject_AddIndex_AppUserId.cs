using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ChatObject_AddIndex_AppUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_AppUserId",
                table: "Chat_ChatObject",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_CreationTime",
                table: "Chat_ChatObject",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_Gender",
                table: "Chat_ChatObject",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_IsDefault",
                table: "Chat_ChatObject",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_IsDeveloper",
                table: "Chat_ChatObject",
                column: "IsDeveloper");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_IsEnabled",
                table: "Chat_ChatObject",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_IsPublic",
                table: "Chat_ChatObject",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_IsStatic",
                table: "Chat_ChatObject",
                column: "IsStatic");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_ObjectType",
                table: "Chat_ChatObject",
                column: "ObjectType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_AppUserId",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_CreationTime",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_Gender",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_IsDefault",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_IsDeveloper",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_IsEnabled",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_IsPublic",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_IsStatic",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_ObjectType",
                table: "Chat_ChatObject");
        }
    }
}
