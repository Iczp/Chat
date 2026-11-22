using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClearBeforeTime",
                table: "Chat_SessionUnitDisposal",
                type: "datetime2",
                nullable: true,
                comment: "历史消息可见性时间截断边界");

            migrationBuilder.AddColumn<long>(
                name: "ClearMaxMessageId",
                table: "Chat_SessionUnitDisposal",
                type: "bigint",
                nullable: true,
                comment: "历史消息可见性最大 ID 截断边界");

            migrationBuilder.CreateTable(
                name: "ChatSessionAnnouncement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    PublisherSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublisherChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    EditorType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    PinnedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPopup = table.Column<bool>(type: "bit", nullable: false),
                    RequireConfirm = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedCount = table.Column<int>(type: "int", nullable: false),
                    ReadCount = table.Column<int>(type: "int", nullable: false),
                    LinkedMessageId = table.Column<long>(type: "bigint", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessionAnnouncement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncement_Chat_ChatObject_ChatObjectId",
                        column: x => x.ChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncement_Chat_ChatObject_PublisherChatObjectId",
                        column: x => x.PublisherChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncement_Chat_SessionUnit_PublisherSessionUnitId",
                        column: x => x.PublisherSessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncement_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatSessionAnnouncementReceipt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    FirstReadTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessionAnnouncementReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncementReceipt_ChatSessionAnnouncement_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "ChatSessionAnnouncement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncementReceipt_Chat_ChatObject_ChatObjectId",
                        column: x => x.ChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncementReceipt_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_ChatObjectId",
                table: "ChatSessionAnnouncement",
                column: "ChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_PublisherChatObjectId",
                table: "ChatSessionAnnouncement",
                column: "PublisherChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_PublisherSessionUnitId",
                table: "ChatSessionAnnouncement",
                column: "PublisherSessionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_SessionId_CreationTime",
                table: "ChatSessionAnnouncement",
                columns: new[] { "SessionId", "CreationTime" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_SessionId_IsPinned",
                table: "ChatSessionAnnouncement",
                columns: new[] { "SessionId", "IsPinned" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementReceipt_AnnouncementId_IsConfirmed",
                table: "ChatSessionAnnouncementReceipt",
                columns: new[] { "AnnouncementId", "IsConfirmed" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementReceipt_AnnouncementId_SessionUnitId",
                table: "ChatSessionAnnouncementReceipt",
                columns: new[] { "AnnouncementId", "SessionUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementReceipt_ChatObjectId",
                table: "ChatSessionAnnouncementReceipt",
                column: "ChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementReceipt_SessionUnitId",
                table: "ChatSessionAnnouncementReceipt",
                column: "SessionUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatSessionAnnouncementReceipt");

            migrationBuilder.DropTable(
                name: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ClearBeforeTime",
                table: "Chat_SessionUnitDisposal");

            migrationBuilder.DropColumn(
                name: "ClearMaxMessageId",
                table: "Chat_SessionUnitDisposal");
        }
    }
}
