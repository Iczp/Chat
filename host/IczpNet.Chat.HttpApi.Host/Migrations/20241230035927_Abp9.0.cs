using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Abp90 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "InviterId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                comment: "邀请人",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "邀请人Id");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_Session",
                type: "bigint",
                nullable: true,
                comment: "Owner",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "InviterId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true,
                comment: "邀请人Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "邀请人");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_Session",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Owner");
        }
    }
}
