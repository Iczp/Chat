using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class DeviceGroup_Add_Description : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "Chat_DeviceGroup",
                comment: "设备分组");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_DeviceGroup",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeviceGroup_Name",
                table: "Chat_DeviceGroup",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_DeviceGroup_Name",
                table: "Chat_DeviceGroup");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_DeviceGroup");

            migrationBuilder.AlterTable(
                name: "Chat_DeviceGroup",
                oldComment: "设备分组");
        }
    }
}
