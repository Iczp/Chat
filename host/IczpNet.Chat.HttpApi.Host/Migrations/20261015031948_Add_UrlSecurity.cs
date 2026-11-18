using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Add_UrlSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SecurityReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterChatObjectId = table.Column<long>(type: "bigint", nullable: true, comment: "举报人 ChatObjectId"),
                    ReporterSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "举报人 SessionUnitId"),
                    TargetType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "投诉目标类型：url / message / user"),
                    TargetContent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false, comment: "投诉目标内容"),
                    MessageId = table.Column<long>(type: "bigint", nullable: true, comment: "关联消息ID"),
                    Reason = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "投诉原因"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "详细描述"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "处理状态：0=待审核, 1=已确认并封禁, 2=驳回/忽略"),
                    AuditRemark = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, comment: "审核意见"),
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
                    table.PrimaryKey("PK_Chat_SecurityReport", x => x.Id);
                },
                comment: "违规投诉举报记录表");

            migrationBuilder.CreateTable(
                name: "Chat_UrlSecurityRule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, comment: "域名模式"),
                    Level = table.Column<int>(type: "int", nullable: false, comment: "安全级别"),
                    Category = table.Column<int>(type: "int", nullable: false, comment: "风险分类"),
                    Reason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "拦截或放行原因说明"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "是否启用"),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "规则有效期至"),
                    HitCount = table.Column<long>(type: "bigint", nullable: false, comment: "命中计数"),
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
                    table.PrimaryKey("PK_Chat_UrlSecurityRule", x => x.Id);
                },
                comment: "URL 域名安全风控规则表");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SecurityReport_CreationTime",
                table: "Chat_SecurityReport",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SecurityReport_Status",
                table: "Chat_SecurityReport",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SecurityReport_TargetContent",
                table: "Chat_SecurityReport",
                column: "TargetContent");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SecurityReport_TargetType",
                table: "Chat_SecurityReport",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UrlSecurityRule_Host",
                table: "Chat_UrlSecurityRule",
                column: "Host");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UrlSecurityRule_Host_IsEnabled",
                table: "Chat_UrlSecurityRule",
                columns: new[] { "Host", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UrlSecurityRule_IsEnabled",
                table: "Chat_UrlSecurityRule",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UrlSecurityRule_Level",
                table: "Chat_UrlSecurityRule",
                column: "Level");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SecurityReport");

            migrationBuilder.DropTable(
                name: "Chat_UrlSecurityRule");
        }
    }
}
