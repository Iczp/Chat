using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Session_AddProp_MessageTotalCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SessionKey",
                table: "Chat_Session",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                comment: "SessionKey",
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnableSetImmersed",
                table: "Chat_Session",
                type: "bit",
                nullable: false,
                comment: "是否可以设置为'免打扰'",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_Session",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "Description",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                comment: "Channel",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MessageTotalCount",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "消息总数量");

            migrationBuilder.AddColumn<DateTime>(
                name: "MessageTotalCountUpdateTime",
                table: "Chat_Session",
                type: "datetime2",
                nullable: true,
                comment: "更新消息总数量时间");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_MessageTotalCount",
                table: "Chat_Session",
                column: "MessageTotalCount",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_MessageTotalCountUpdateTime",
                table: "Chat_Session",
                column: "MessageTotalCountUpdateTime",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_MessageTotalCount",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_MessageTotalCountUpdateTime",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "MessageTotalCount",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "MessageTotalCountUpdateTime",
                table: "Chat_Session");

            migrationBuilder.AlterColumn<string>(
                name: "SessionKey",
                table: "Chat_Session",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true,
                oldComment: "SessionKey");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnableSetImmersed",
                table: "Chat_Session",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "是否可以设置为'免打扰'");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_Session",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Description");

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Channel");
        }
    }
}
