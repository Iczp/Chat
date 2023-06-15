using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Session_AddProp_IsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "KillerId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                comment: "删除人",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "KillType",
                table: "Chat_SessionUnitSetting",
                type: "int",
                nullable: true,
                comment: "删除渠道",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Session",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Session");

            migrationBuilder.AlterColumn<Guid>(
                name: "KillerId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "删除人");

            migrationBuilder.AlterColumn<int>(
                name: "KillType",
                table: "Chat_SessionUnitSetting",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "删除渠道");
        }
    }
}
