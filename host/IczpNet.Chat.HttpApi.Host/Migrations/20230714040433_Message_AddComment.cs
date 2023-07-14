using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                comment: "消息大小kb",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionUnitId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                comment: "会话单元Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                comment: "会话Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SenderType",
                table: "Chat_Message",
                type: "int",
                nullable: true,
                comment: "发送者类型",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SenderId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                comment: "发送者",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RollbackTime",
                table: "Chat_Message",
                type: "datetime2",
                nullable: true,
                comment: "撤回消息时间",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReceiverType",
                table: "Chat_Message",
                type: "int",
                nullable: true,
                comment: "接收者类型",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ReceiverId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                comment: "接收者",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MessageType",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                comment: "消息类型",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "KeyValue",
                table: "Chat_Message",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                comment: "扩展（键值）根据业务自义",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "扩展（键名）根据业务自义",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRollbacked",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                comment: "是否撤回",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrivate",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                comment: "私有消息(只有发送人[senderId]和接收人[receiverId]才能看)",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ContentJson",
                table: "Chat_Message",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                comment: "ContentJson",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                comment: "消息通道",
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "消息大小kb");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionUnitId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "会话单元Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "会话Id");

            migrationBuilder.AlterColumn<int>(
                name: "SenderType",
                table: "Chat_Message",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "发送者类型");

            migrationBuilder.AlterColumn<long>(
                name: "SenderId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "发送者");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RollbackTime",
                table: "Chat_Message",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "撤回消息时间");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiverType",
                table: "Chat_Message",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "接收者类型");

            migrationBuilder.AlterColumn<long>(
                name: "ReceiverId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "接收者");

            migrationBuilder.AlterColumn<int>(
                name: "MessageType",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "消息类型");

            migrationBuilder.AlterColumn<string>(
                name: "KeyValue",
                table: "Chat_Message",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true,
                oldComment: "扩展（键值）根据业务自义");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "扩展（键名）根据业务自义");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRollbacked",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "是否撤回");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrivate",
                table: "Chat_Message",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "私有消息(只有发送人[senderId]和接收人[receiverId]才能看)");

            migrationBuilder.AlterColumn<string>(
                name: "ContentJson",
                table: "Chat_Message",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true,
                oldComment: "ContentJson");

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "消息通道");
        }
    }
}
