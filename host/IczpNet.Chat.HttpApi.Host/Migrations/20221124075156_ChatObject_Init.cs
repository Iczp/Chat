using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatObject_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ChatObject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatObject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageChannel = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    ForwardMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuoteMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuoteDepth = table.Column<long>(type: "bigint", nullable: false),
                    ForwardDepth = table.Column<long>(type: "bigint", nullable: false),
                    KeyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KeyValue = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    RollbackTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_ChatObject_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_ChatObject_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_Message_ForwardMessageId",
                        column: x => x.ForwardMessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_Message_QuoteMessageId",
                        column: x => x.QuoteMessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ForwardMessageId",
                table: "Chat_Message",
                column: "ForwardMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_QuoteMessageId",
                table: "Chat_Message",
                column: "QuoteMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ReceiverId",
                table: "Chat_Message",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderId",
                table: "Chat_Message",
                column: "SenderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_Message");

            migrationBuilder.DropTable(
                name: "Chat_ChatObject");
        }
    }
}
