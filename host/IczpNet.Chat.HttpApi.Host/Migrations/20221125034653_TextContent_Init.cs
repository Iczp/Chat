using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class TextContent_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_Message_TextMessage");

            migrationBuilder.DropTable(
                name: "Chat_TextMessage");

            migrationBuilder.CreateTable(
                name: "Chat_TextContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_TextContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_TextContent",
                columns: table => new
                {
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TextMessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_TextContent", x => new { x.MessageListId, x.TextMessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_TextContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_TextContent_Chat_TextContent_TextMessageListId",
                        column: x => x.TextMessageListId,
                        principalTable: "Chat_TextContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_TextContent_TextMessageListId",
                table: "Chat_Message_TextContent",
                column: "TextMessageListId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_Message_TextContent");

            migrationBuilder.DropTable(
                name: "Chat_TextContent");

            migrationBuilder.CreateTable(
                name: "Chat_TextMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_TextMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_TextMessage",
                columns: table => new
                {
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TextMessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_TextMessage", x => new { x.MessageListId, x.TextMessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_TextMessage_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_TextMessage_Chat_TextMessage_TextMessageListId",
                        column: x => x.TextMessageListId,
                        principalTable: "Chat_TextMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_TextMessage_TextMessageListId",
                table: "Chat_Message_TextMessage",
                column: "TextMessageListId");
        }
    }
}
