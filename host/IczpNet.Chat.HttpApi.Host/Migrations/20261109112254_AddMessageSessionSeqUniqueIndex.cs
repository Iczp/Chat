using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageSessionSeqUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "SessionSeq",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                comment: "当前消息在会话内的序号",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L,
                oldComment: "当前消息在会话内的序号");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_SessionSeq",
                table: "Chat_Message",
                columns: new[] { "SessionId", "SessionSeq" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_SessionSeq",
                table: "Chat_Message");

            migrationBuilder.AlterColumn<long>(
                name: "SessionSeq",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "当前消息在会话内的序号",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "当前消息在会话内的序号");
        }
    }
}
