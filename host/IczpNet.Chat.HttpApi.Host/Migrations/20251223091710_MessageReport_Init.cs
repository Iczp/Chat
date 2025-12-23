using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageReport_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageStat");

            migrationBuilder.CreateTable(
                name: "Chat_MessageReportDay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话Id"),
                    MessageType = table.Column<int>(type: "int", nullable: false, comment: "消息类型"),
                    DateBucket = table.Column<long>(type: "bigint", nullable: false, comment: "日期(数字)"),
                    Count = table.Column<long>(type: "bigint", nullable: false, comment: "数量")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageReportDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_MessageReportDay_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_MessageReportHour",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话Id"),
                    MessageType = table.Column<int>(type: "int", nullable: false, comment: "消息类型"),
                    DateBucket = table.Column<long>(type: "bigint", nullable: false, comment: "日期(数字)"),
                    Count = table.Column<long>(type: "bigint", nullable: false, comment: "数量")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageReportHour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_MessageReportHour_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_MessageReportMonth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话Id"),
                    MessageType = table.Column<int>(type: "int", nullable: false, comment: "消息类型"),
                    DateBucket = table.Column<long>(type: "bigint", nullable: false, comment: "日期(数字)"),
                    Count = table.Column<long>(type: "bigint", nullable: false, comment: "数量")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageReportMonth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_MessageReportMonth_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_MessageType",
                table: "Chat_MessageReportDay",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_SessionId",
                table: "Chat_MessageReportDay",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_SessionId_DateBucket_MessageType",
                table: "Chat_MessageReportDay",
                columns: new[] { "SessionId", "DateBucket", "MessageType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_SessionId_MessageType",
                table: "Chat_MessageReportDay",
                columns: new[] { "SessionId", "MessageType" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_MessageType",
                table: "Chat_MessageReportHour",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_SessionId",
                table: "Chat_MessageReportHour",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_SessionId_DateBucket_MessageType",
                table: "Chat_MessageReportHour",
                columns: new[] { "SessionId", "DateBucket", "MessageType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_SessionId_MessageType",
                table: "Chat_MessageReportHour",
                columns: new[] { "SessionId", "MessageType" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_MessageType",
                table: "Chat_MessageReportMonth",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_SessionId",
                table: "Chat_MessageReportMonth",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_SessionId_DateBucket_MessageType",
                table: "Chat_MessageReportMonth",
                columns: new[] { "SessionId", "DateBucket", "MessageType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_SessionId_MessageType",
                table: "Chat_MessageReportMonth",
                columns: new[] { "SessionId", "MessageType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageReportDay");

            migrationBuilder.DropTable(
                name: "Chat_MessageReportHour");

            migrationBuilder.DropTable(
                name: "Chat_MessageReportMonth");

            migrationBuilder.CreateTable(
                name: "Chat_MessageStat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话Id"),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false, comment: "数量"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateBucket = table.Column<long>(type: "bigint", nullable: false, comment: "日期(数字)"),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageType = table.Column<int>(type: "int", nullable: false, comment: "消息类型")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageStat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_MessageStat_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_MessageType",
                table: "Chat_MessageStat",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId",
                table: "Chat_MessageStat",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_DateBucket_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "DateBucket", "MessageType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "MessageType" });
        }
    }
}
