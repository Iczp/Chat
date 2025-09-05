using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageFollower : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderFollerIds",
                table: "Chat_Message",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                comment: "发送人的关注者(粉丝)");

            migrationBuilder.CreateTable(
                name: "Chat_MessageFollower",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageFollower", x => new { x.SessionUnitId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_Chat_MessageFollower_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_MessageFollower_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "消息关注者");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageFollower_MessageId",
                table: "Chat_MessageFollower",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageFollower");

            migrationBuilder.DropColumn(
                name: "SenderFollerIds",
                table: "Chat_Message");
        }
    }
}
