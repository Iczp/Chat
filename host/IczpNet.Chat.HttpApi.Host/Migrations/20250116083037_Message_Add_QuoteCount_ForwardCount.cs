using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_Add_QuoteCount_ForwardCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SessionUnitCount",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                comment: "会话单元数量",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SessionKey",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "SessionKey",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReminderType",
                table: "Chat_Message",
                type: "int",
                nullable: true,
                comment: "提醒器类型",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QuotePath",
                table: "Chat_Message",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "引用层级",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "QuoteMessageId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                comment: "引用来源Id(引用才有)",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "QuoteDepth",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                comment: "引用层级 0:不是引用",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "提供者",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsScoped",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                comment: "指定范围",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemindAll",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                comment: "@所有人 Remind Everyone ",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ForwardPath",
                table: "Chat_Message",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "转发层级",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ForwardMessageId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                comment: "转发来源Id(转发才有)",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ForwardDepth",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                comment: "转发层级 0:不是转发",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ForwardCount",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "被转发次数");

            migrationBuilder.AddColumn<long>(
                name: "QuoteCount",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "被引用次数");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ForwardCount",
                table: "Chat_Message",
                column: "ForwardCount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ForwardPath",
                table: "Chat_Message",
                column: "ForwardPath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_QuoteCount",
                table: "Chat_Message",
                column: "QuoteCount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_QuotePath",
                table: "Chat_Message",
                column: "QuotePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ForwardCount",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ForwardPath",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_QuoteCount",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_QuotePath",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ForwardCount",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "QuoteCount",
                table: "Chat_Message");

            migrationBuilder.AlterColumn<int>(
                name: "SessionUnitCount",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "会话单元数量");

            migrationBuilder.AlterColumn<string>(
                name: "SessionKey",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "SessionKey");

            migrationBuilder.AlterColumn<int>(
                name: "ReminderType",
                table: "Chat_Message",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "提醒器类型");

            migrationBuilder.AlterColumn<string>(
                name: "QuotePath",
                table: "Chat_Message",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "引用层级");

            migrationBuilder.AlterColumn<long>(
                name: "QuoteMessageId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "引用来源Id(引用才有)");

            migrationBuilder.AlterColumn<long>(
                name: "QuoteDepth",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "引用层级 0:不是引用");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "提供者");

            migrationBuilder.AlterColumn<bool>(
                name: "IsScoped",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "指定范围");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemindAll",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "@所有人 Remind Everyone ");

            migrationBuilder.AlterColumn<string>(
                name: "ForwardPath",
                table: "Chat_Message",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "转发层级");

            migrationBuilder.AlterColumn<long>(
                name: "ForwardMessageId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "转发来源Id(转发才有)");

            migrationBuilder.AlterColumn<long>(
                name: "ForwardDepth",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "转发层级 0:不是转发");
        }
    }
}
