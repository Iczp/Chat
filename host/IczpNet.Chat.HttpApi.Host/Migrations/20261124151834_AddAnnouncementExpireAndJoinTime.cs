using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementExpireAndJoinTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyToNewMembers",
                table: "ChatSessionAnnouncement",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "是否对发布后新入群/重入群成员生效（弹窗/需确认）");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireTime",
                table: "ChatSessionAnnouncement",
                type: "datetime2",
                nullable: true,
                comment: "公告有效期截止时间（null=长期有效）");

            migrationBuilder.AddColumn<double>(
                name: "Sorting",
                table: "ChatSessionAnnouncement",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                comment: "置顶排序权重（0=不置顶，数值越大越置顶）");

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinTime",
                table: "Chat_SessionUnitSetting",
                type: "datetime2",
                nullable: true,
                comment: "加入时间");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_ExpireTime",
                table: "ChatSessionAnnouncement",
                column: "ExpireTime");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_SessionId_Sorting",
                table: "ChatSessionAnnouncement",
                columns: new[] { "SessionId", "Sorting" },
                descending: new bool[0]);

            migrationBuilder.Sql(
                "UPDATE [ChatSessionAnnouncement] SET [Sorting] = 1 WHERE [IsPinned] = 1 AND [Sorting] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSessionAnnouncement_ExpireTime",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessionAnnouncement_SessionId_Sorting",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ApplyToNewMembers",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ExpireTime",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "JoinTime",
                table: "Chat_SessionUnitSetting");
        }
    }
}
