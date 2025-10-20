using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddIndex_OwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_ReadedMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit");

            migrationBuilder.AlterColumn<string>(
                name: "Platform",
                table: "Chat_ClientConfig",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "DeviceId",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "Chat_ClientConfig",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "DeviceId",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_IsImmersed",
                table: "Chat_SessionUnitSetting",
                column: "IsImmersed",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_ReadedMessageId",
                table: "Chat_SessionUnitSetting",
                column: "ReadedMessageId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId_PublicBadge_PrivateBadge",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "PublicBadge", "PrivateBadge" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_IsImmersed",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_ReadedMessageId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_PublicBadge_PrivateBadge",
                table: "Chat_SessionUnit");

            migrationBuilder.AlterColumn<string>(
                name: "Platform",
                table: "Chat_ClientConfig",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "DeviceId");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "Chat_ClientConfig",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_ReadedMessageId",
                table: "Chat_SessionUnitSetting",
                column: "ReadedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId");
        }
    }
}
