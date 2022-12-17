using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionMember");

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnit",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReadedMessageAutoId = table.Column<long>(type: "bigint", nullable: false),
                    HistoryFristTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnit", x => new { x.SessionId, x.OwnerId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_DestinationId",
                table: "Chat_SessionUnit",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnit");

            migrationBuilder.CreateTable(
                name: "Chat_SessionMember",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoryFristTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReadedMessageAutoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionMember", x => new { x.SessionId, x.OwnerId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionMember_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionMember_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionMember_DestinationId",
                table: "Chat_SessionMember",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionMember_OwnerId",
                table: "Chat_SessionMember",
                column: "OwnerId");
        }
    }
}
