using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Connection_Reconstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ServerHost",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ActiveTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_Chat_ServerHost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Connection",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ServerHostId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChatObjects = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ActiveTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_Chat_Connection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Connection_Chat_ServerHost_ServerHostId",
                        column: x => x.ServerHostId,
                        principalTable: "Chat_ServerHost",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_ConnectionChatObject",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    ChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ConnectionChatObject", x => new { x.ChatObjectId, x.ConnectionId });
                    table.ForeignKey(
                        name: "FK_Chat_ConnectionChatObject_Chat_ChatObject_ChatObjectId",
                        column: x => x.ChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_ConnectionChatObject_Chat_Connection_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Chat_Connection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Connection_ActiveTime",
                table: "Chat_Connection",
                column: "ActiveTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Connection_ChatObjects",
                table: "Chat_Connection",
                column: "ChatObjects");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Connection_DeviceId",
                table: "Chat_Connection",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Connection_IpAddress",
                table: "Chat_Connection",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Connection_ServerHostId",
                table: "Chat_Connection",
                column: "ServerHostId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ConnectionChatObject_ConnectionId",
                table: "Chat_ConnectionChatObject",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ServerHost_ActiveTime",
                table: "Chat_ServerHost",
                column: "ActiveTime",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ConnectionChatObject");

            migrationBuilder.DropTable(
                name: "Chat_Connection");

            migrationBuilder.DropTable(
                name: "Chat_ServerHost");
        }
    }
}
