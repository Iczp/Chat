using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageStat_Fix_MessageType_Int : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MessageType",
                table: "Chat_MessageStat",
                type: "int",
                nullable: false,
                comment: "消息类型",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldComment: "消息类型");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MessageType",
                table: "Chat_MessageStat",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                comment: "消息类型",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "消息类型");
        }
    }
}
