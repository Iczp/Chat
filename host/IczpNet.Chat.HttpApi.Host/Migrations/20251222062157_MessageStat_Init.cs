using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageStat_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_DeletedRecorder_MessageUnit",
                table: "Chat_DeletedRecorder");

            migrationBuilder.CreateTable(
                name: "Chat_MessageStat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键 日期"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话Id"),
                    MessageType = table.Column<int>(type: "int", maxLength: 36, nullable: false, comment: "消息类型"),
                    Count = table.Column<long>(type: "bigint", nullable: false, comment: "数量"),
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
                    table.PrimaryKey("PK_Chat_MessageStat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_MessageStat_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_MessageId",
                table: "Chat_DeletedRecorder",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "MessageType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageStat");

            migrationBuilder.DropIndex(
                name: "IX_Chat_DeletedRecorder_MessageId",
                table: "Chat_DeletedRecorder");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_MessageUnit",
                table: "Chat_DeletedRecorder",
                columns: new[] { "MessageId", "SessionUnitId" });
        }
    }
}
