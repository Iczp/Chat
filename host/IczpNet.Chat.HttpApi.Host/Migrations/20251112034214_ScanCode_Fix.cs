using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ScanCode_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandlerFullName",
                table: "Chat_ScanHandler",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Execution",
                table: "Chat_ScanCode",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                comment: "执行时间（毫秒）");

            migrationBuilder.AddColumn<int>(
                name: "HandlerCount",
                table: "Chat_ScanCode",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "处理器个数");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandlerFullName",
                table: "Chat_ScanHandler");

            migrationBuilder.DropColumn(
                name: "Execution",
                table: "Chat_ScanCode");

            migrationBuilder.DropColumn(
                name: "HandlerCount",
                table: "Chat_ScanCode");
        }
    }
}
