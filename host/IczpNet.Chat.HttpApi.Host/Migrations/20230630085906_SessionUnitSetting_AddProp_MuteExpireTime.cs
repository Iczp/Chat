using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_AddProp_MuteExpireTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MuteExpireTime",
                table: "Chat_SessionUnitSetting",
                type: "datetime2",
                nullable: true,
                comment: "禁言过期时间，为空则不禁言");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_MuteExpireTime",
                table: "Chat_SessionUnitSetting",
                column: "MuteExpireTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_MuteExpireTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "MuteExpireTime",
                table: "Chat_SessionUnitSetting");
        }
    }
}
