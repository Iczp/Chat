using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Device_Fix_DeviceOrientation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Ua",
                table: "Chat_Device",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                comment: "用户标识（小程序为空）",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "用户标识（小程序为空）");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceOrientation",
                table: "Chat_Device",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "设备方向",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "设备方向");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Ua",
                table: "Chat_Device",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "用户标识（小程序为空）",
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true,
                oldComment: "用户标识（小程序为空）");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceOrientation",
                table: "Chat_Device",
                type: "int",
                nullable: true,
                comment: "设备方向",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "设备方向");
        }
    }
}
