using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Device_Fix_AppVersionCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "AppVersionCode",
                table: "Chat_Device",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "manifest.json 中应用版本号",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "manifest.json 中应用版本号");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AppVersionCode",
                table: "Chat_Device",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "manifest.json 中应用版本号",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "manifest.json 中应用版本号");
        }
    }
}
