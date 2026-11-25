using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementReviewVersionAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfirmedVersion",
                table: "ChatSessionAnnouncementReceipt",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReadVersion",
                table: "ChatSessionAnnouncementReceipt",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ChatSessionAnnouncement",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "是否已通过审核（具备直发权或审核通过为 true）");

            migrationBuilder.AddColumn<string>(
                name: "ReviewRemark",
                table: "ChatSessionAnnouncement",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedTime",
                table: "ChatSessionAnnouncement",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReviewerChatObjectId",
                table: "ChatSessionAnnouncement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewerSessionUnitId",
                table: "ChatSessionAnnouncement",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ChatSessionAnnouncement",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "当前公告版本号（未发布=0，首次发布=1，升版递增）");

            migrationBuilder.CreateTable(
                name: "ChatSessionAnnouncementRevision",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    EditorType = table.Column<int>(type: "int", nullable: false),
                    EditorSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditorChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("PK_ChatSessionAnnouncementRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncementRevision_ChatSessionAnnouncement_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "ChatSessionAnnouncement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncementRevision_Chat_ChatObject_EditorChatObjectId",
                        column: x => x.EditorChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessionAnnouncementRevision_Chat_SessionUnit_EditorSessionUnitId",
                        column: x => x.EditorSessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_ReviewerChatObjectId",
                table: "ChatSessionAnnouncement",
                column: "ReviewerChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncement_ReviewerSessionUnitId",
                table: "ChatSessionAnnouncement",
                column: "ReviewerSessionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementRevision_AnnouncementId_Version",
                table: "ChatSessionAnnouncementRevision",
                columns: new[] { "AnnouncementId", "Version" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementRevision_EditorChatObjectId",
                table: "ChatSessionAnnouncementRevision",
                column: "EditorChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessionAnnouncementRevision_EditorSessionUnitId",
                table: "ChatSessionAnnouncementRevision",
                column: "EditorSessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessionAnnouncement_Chat_ChatObject_ReviewerChatObjectId",
                table: "ChatSessionAnnouncement",
                column: "ReviewerChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessionAnnouncement_Chat_SessionUnit_ReviewerSessionUnitId",
                table: "ChatSessionAnnouncement",
                column: "ReviewerSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
UPDATE [ChatSessionAnnouncement] SET [Version] = 1, [IsApproved] = 1 WHERE [Status] = 1 AND [Version] = 0;
UPDATE [ChatSessionAnnouncement] SET [IsApproved] = 1 WHERE [Status] = 2 AND [IsApproved] = 0;
UPDATE [ChatSessionAnnouncementReceipt] SET [ReadVersion] = 1 WHERE [ReadVersion] = 0;
UPDATE [ChatSessionAnnouncementReceipt] SET [ConfirmedVersion] = 1 WHERE [IsConfirmed] = 1 AND [ConfirmedVersion] = 0;

DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @defs TABLE ([Id] nvarchar(450), [Name] nvarchar(120), [Sorting] bigint);
INSERT INTO @defs ([Id], [Name], [Sorting]) VALUES
(N'ChatSessionPermission.AnnouncementPermission.Create', N'创建/提审群公告', 100),
(N'ChatSessionPermission.AnnouncementPermission.Publish', N'直接发布群公告', 99),
(N'ChatSessionPermission.AnnouncementPermission.Approve', N'审核群公告', 98),
(N'ChatSessionPermission.AnnouncementPermission.Update', N'管理/编辑群公告', 97),
(N'ChatSessionPermission.AnnouncementPermission.UpdateOwn', N'编辑/撤回本人公告', 96),
(N'ChatSessionPermission.AnnouncementPermission.UpdateAny', N'编辑/撤回他人公告', 95),
(N'ChatSessionPermission.AnnouncementPermission.Delete', N'删除群公告', 94),
(N'ChatSessionPermission.AnnouncementPermission.DeleteOwn', N'删除本人公告', 93),
(N'ChatSessionPermission.AnnouncementPermission.DeleteAny', N'删除他人公告', 92),
(N'ChatSessionPermission.AnnouncementPermission.Pin', N'置顶/调权群公告', 91),
(N'ChatSessionPermission.AnnouncementPermission.Remind', N'催办未确认成员', 90),
(N'ChatSessionPermission.AnnouncementPermission.ViewStat', N'查看公告回执统计', 89);

UPDATE d
SET d.[Name] = src.[Name], d.[IsEnabled] = 1
FROM [Chat_SessionPermissionDefinition] d
INNER JOIN @defs src ON d.[Id] = src.[Id];

INSERT INTO [Chat_SessionPermissionDefinition]
([Id], [ConcurrencyStamp], [CreationTime], [DefaultValue], [Description], [ExtraProperties], [IsDeleted], [IsEnabled], [MaxValue], [MinValue], [Name], [Sorting])
SELECT src.[Id], LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), '-', '')), @now, 0, N'AnnouncementPermission', N'{}', 0, 1, 1, 0, src.[Name], src.[Sorting]
FROM @defs src
WHERE NOT EXISTS (SELECT 1 FROM [Chat_SessionPermissionDefinition] d WHERE d.[Id] = src.[Id]);

DECLARE @expand TABLE ([DefId] nvarchar(450));
INSERT INTO @expand ([DefId]) VALUES
(N'ChatSessionPermission.AnnouncementPermission.Publish'),
(N'ChatSessionPermission.AnnouncementPermission.Approve'),
(N'ChatSessionPermission.AnnouncementPermission.UpdateOwn'),
(N'ChatSessionPermission.AnnouncementPermission.DeleteOwn'),
(N'ChatSessionPermission.AnnouncementPermission.Remind'),
(N'ChatSessionPermission.AnnouncementPermission.ViewStat');

INSERT INTO [Chat_SessionPermissionRoleGrant]
([DefinitionId], [RoleId], [ConcurrencyStamp], [CreationTime], [ExtraProperties], [IsEnabled], [Value])
SELECT e.[DefId], rg.[RoleId], LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), '-', '')), @now, N'{}', rg.[IsEnabled], rg.[Value]
FROM [Chat_SessionPermissionRoleGrant] rg
CROSS JOIN @expand e
WHERE rg.[DefinitionId] = N'ChatSessionPermission.AnnouncementPermission.Update'
  AND rg.[IsEnabled] = 1
  AND NOT EXISTS (
      SELECT 1 FROM [Chat_SessionPermissionRoleGrant] existing
      WHERE existing.[DefinitionId] = e.[DefId] AND existing.[RoleId] = rg.[RoleId]
  );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessionAnnouncement_Chat_ChatObject_ReviewerChatObjectId",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessionAnnouncement_Chat_SessionUnit_ReviewerSessionUnitId",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropTable(
                name: "ChatSessionAnnouncementRevision");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessionAnnouncement_ReviewerChatObjectId",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessionAnnouncement_ReviewerSessionUnitId",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ConfirmedVersion",
                table: "ChatSessionAnnouncementReceipt");

            migrationBuilder.DropColumn(
                name: "ReadVersion",
                table: "ChatSessionAnnouncementReceipt");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ReviewRemark",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ReviewedTime",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ReviewerChatObjectId",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "ReviewerSessionUnitId",
                table: "ChatSessionAnnouncement");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ChatSessionAnnouncement");
        }
    }
}
