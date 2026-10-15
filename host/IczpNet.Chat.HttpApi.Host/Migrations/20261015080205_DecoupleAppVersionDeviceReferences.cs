using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleAppVersionDeviceReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_Device_DeviceId",
                table: "Chat_AppVersionDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_Device_DeviceId",
                table: "Chat_AppVersionDevice",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup",
                column: "DeviceGroupId",
                principalTable: "Chat_DeviceGroup",
                principalColumn: "Id");
        }
    }
}
