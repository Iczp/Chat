using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionRoomPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowJoinChannels",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                defaultValue: 7,
                comment: "允许的入群渠道");

            migrationBuilder.AddColumn<int>(
                name: "InvitePolicy",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "群邀请策略");

            migrationBuilder.AddColumn<bool>(
                name: "IsAllMuted",
                table: "Chat_Session",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "全员禁言");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowJoinChannels",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "InvitePolicy",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "IsAllMuted",
                table: "Chat_Session");
        }
    }
}
