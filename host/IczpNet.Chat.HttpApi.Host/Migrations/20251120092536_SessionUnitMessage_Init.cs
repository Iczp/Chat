using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitMessage_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitMessage",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsOpened = table.Column<bool>(type: "bit", nullable: false),
                    IsFavorited = table.Column<bool>(type: "bit", nullable: false),
                    IsFollowing = table.Column<bool>(type: "bit", nullable: false),
                    IsRemindMe = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitMessage", x => new { x.SessionUnitId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitMessage_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitMessage_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_MessageId",
                table: "Chat_SessionUnitMessage",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsOpened",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "SessionUnitId", "IsOpened" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_IsRead",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "SessionUnitId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitMessage_SessionUnitId_MessageId",
                table: "Chat_SessionUnitMessage",
                columns: new[] { "SessionUnitId", "MessageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitMessage");
        }
    }
}
