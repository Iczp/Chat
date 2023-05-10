using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_FixStatProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReminderMeCount",
                table: "Chat_SessionUnit",
                newName: "RemindMeCount");

            migrationBuilder.RenameColumn(
                name: "ReminderAllCount",
                table: "Chat_SessionUnit",
                newName: "RemindAllCount");

            migrationBuilder.RenameColumn(
                name: "BadgePublic",
                table: "Chat_SessionUnit",
                newName: "PublicBadge");

            migrationBuilder.RenameColumn(
                name: "BadgePrivate",
                table: "Chat_SessionUnit",
                newName: "PrivateBadge");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemindMeCount",
                table: "Chat_SessionUnit",
                newName: "ReminderMeCount");

            migrationBuilder.RenameColumn(
                name: "RemindAllCount",
                table: "Chat_SessionUnit",
                newName: "ReminderAllCount");

            migrationBuilder.RenameColumn(
                name: "PublicBadge",
                table: "Chat_SessionUnit",
                newName: "BadgePublic");

            migrationBuilder.RenameColumn(
                name: "PrivateBadge",
                table: "Chat_SessionUnit",
                newName: "BadgePrivate");
        }
    }
}
