using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddStatProp_BadgePublic_BadgePrivate_ReminderAllCount_ReminderMeCount_FollowingCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BadgePrivate",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BadgePublic",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReminderAllCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReminderMeCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BadgePrivate",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "BadgePublic",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "FollowingCount",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "ReminderAllCount",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "ReminderMeCount",
                table: "Chat_SessionUnit");
        }
    }
}
