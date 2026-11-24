using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementPublishTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishTime",
                table: "ChatSessionAnnouncement",
                type: "datetime2",
                nullable: true,
                comment: "定时发布时间（null=立即发布）");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_Status_PublishTime",
                table: "ChatSessionAnnouncement",
                columns: new[] { "Status", "PublishTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSessionAnnouncement_Status_PublishTime",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "PublishTime",
                table: "ChatSessionAnnouncement");
        }
    }
}
