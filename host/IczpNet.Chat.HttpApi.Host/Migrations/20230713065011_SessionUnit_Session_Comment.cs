using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_Session_Comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionUnitCounter_LastMessageId",
                table: "Chat_SessionUnitCounter",
                newName: "IX_Chat_SessionUnitCounter_LastMessageId_Desc");

            migrationBuilder.AlterTable(
                name: "Chat_SessionUnitSetting",
                comment: "会话设置");

            migrationBuilder.AlterColumn<string>(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "备注拼音简写",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RenameSpelling",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                comment: "备注拼音简写",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "会话内的名称拼音简写",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                comment: "会话内的名称拼音",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "KillerId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                comment: "删除人Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "删除人");

            migrationBuilder.AlterColumn<int>(
                name: "JoinWay",
                table: "Chat_SessionUnitSetting",
                type: "int",
                nullable: true,
                comment: "加入方式",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "InviterId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                comment: "邀请人Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundImage",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "聊天背景，默认为 null",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Ticks",
                table: "Chat_SessionUnit",
                type: "float",
                nullable: false,
                comment: "时间",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Sorting",
                table: "Chat_SessionUnit",
                type: "float",
                nullable: false,
                comment: "置顶(排序)",
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: false,
                comment: "会话Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "RemindMeCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "提醒器数量(@我)",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RemindAllCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "提醒器数量(@所有人)",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PublicBadge",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "最后一条消息Id",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PrivateBadge",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "私有消息角标(未读消息数量)",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "LastMessageId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: true,
                comment: "最后一条消息Id",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FollowingCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                comment: "特别关注数量",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true,
                comment: "用户Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitCounter_LastMessageId",
                table: "Chat_SessionUnitCounter",
                column: "LastMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitCounter_LastMessageId",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionUnitCounter_LastMessageId_Desc",
                table: "Chat_SessionUnitCounter",
                newName: "IX_Chat_SessionUnitCounter_LastMessageId");

            migrationBuilder.AlterTable(
                name: "Chat_SessionUnitSetting",
                oldComment: "会话设置");

            migrationBuilder.AlterColumn<string>(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "备注拼音简写");

            migrationBuilder.AlterColumn<string>(
                name: "RenameSpelling",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true,
                oldComment: "备注拼音简写");

            migrationBuilder.AlterColumn<string>(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "会话内的名称拼音简写");

            migrationBuilder.AlterColumn<string>(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true,
                oldComment: "会话内的名称拼音");

            migrationBuilder.AlterColumn<Guid>(
                name: "KillerId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                comment: "删除人",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "删除人Id");

            migrationBuilder.AlterColumn<int>(
                name: "JoinWay",
                table: "Chat_SessionUnitSetting",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "加入方式");

            migrationBuilder.AlterColumn<Guid>(
                name: "InviterId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "邀请人Id");

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundImage",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "聊天背景，默认为 null");

            migrationBuilder.AlterColumn<double>(
                name: "Ticks",
                table: "Chat_SessionUnit",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComment: "时间");

            migrationBuilder.AlterColumn<double>(
                name: "Sorting",
                table: "Chat_SessionUnit",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComment: "置顶(排序)");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "会话Id");

            migrationBuilder.AlterColumn<int>(
                name: "RemindMeCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "提醒器数量(@我)");

            migrationBuilder.AlterColumn<int>(
                name: "RemindAllCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "提醒器数量(@所有人)");

            migrationBuilder.AlterColumn<int>(
                name: "PublicBadge",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "最后一条消息Id");

            migrationBuilder.AlterColumn<int>(
                name: "PrivateBadge",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "私有消息角标(未读消息数量)");

            migrationBuilder.AlterColumn<long>(
                name: "LastMessageId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "最后一条消息Id");

            migrationBuilder.AlterColumn<int>(
                name: "FollowingCount",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "特别关注数量");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "用户Id");
        }
    }
}
