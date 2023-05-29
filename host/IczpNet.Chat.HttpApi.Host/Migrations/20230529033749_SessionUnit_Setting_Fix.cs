using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_Setting_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationTime",
                table: "Chat_SessionUnitCounter",
                type: "datetime2",
                nullable: true,
                comment: "修改时间",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                table: "Chat_SessionUnitCounter",
                type: "datetime2",
                nullable: false,
                comment: "创建时间",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Chat_SessionUnitCounter",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Chat_SessionUnitCounter",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Chat_SessionUnitCounter",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_SessionUnitCounter",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Chat_SessionUnitCounter",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationTime",
                table: "Chat_SessionUnitCounter",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "修改时间");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                table: "Chat_SessionUnitCounter",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "创建时间");
        }
    }
}
