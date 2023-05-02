using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_Key_AndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Chat_SessionUnit",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_IsPublic",
                table: "Chat_SessionUnit",
                column: "IsPublic",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_IsStatic",
                table: "Chat_SessionUnit",
                column: "IsStatic",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Key",
                table: "Chat_SessionUnit",
                column: "Key",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_IsPublic",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_IsStatic",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Key",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Chat_SessionUnit");
        }
    }
}
