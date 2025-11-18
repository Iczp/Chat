using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class UserDevice_Add_UserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppId",
                table: "Chat_UserDevice",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "AppId");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Chat_UserDevice",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppId",
                table: "Chat_UserDevice");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Chat_UserDevice");
        }
    }
}
