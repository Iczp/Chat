using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_Add_IsHideBadge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHideBadge",
                table: "Chat_SessionUnitSetting",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "是否隐藏角标");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHideBadge",
                table: "Chat_SessionUnitSetting");
        }
    }
}
