using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Remove_MessageStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageStat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_MessageStat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键 日期"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话Id"),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false, comment: "数量"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, comment: "消息类型")
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
                name: "IX_Chat_MessageStat_Id_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "Id", "SessionId", "MessageType" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_MessageType",
                table: "Chat_MessageStat",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId",
                table: "Chat_MessageStat",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "MessageType" });
        }
    }
}
