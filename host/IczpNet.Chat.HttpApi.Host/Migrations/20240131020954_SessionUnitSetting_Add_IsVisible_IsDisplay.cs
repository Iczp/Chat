using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_Add_IsVisible_IsDisplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisplay",
                table: "Chat_SessionUnitSetting",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "是否显示");

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Chat_SessionUnitSetting",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "是否可见的");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisplay",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Chat_SessionUnitSetting");
        }
    }
}
