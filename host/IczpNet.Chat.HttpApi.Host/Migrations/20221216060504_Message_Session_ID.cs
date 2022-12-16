using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_Session_ID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_DestinationId",
                table: "Chat_Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_Message_MessageId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_DestinationId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_DestinationObjectType",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_MessageId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "Badge",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "DestinationObjectType",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "MessageAutoId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "ShowTime",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "Chat_Session");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionIdValue",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_SessionMember",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionMember", x => new { x.SessionId, x.OwnerId });
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
                name: "IX_Chat_Message_SessionIdValue",
                table: "Chat_Message",
                column: "SessionIdValue");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionMember_OwnerId",
                table: "Chat_SessionMember",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionIdValue",
                table: "Chat_Message",
                column: "SessionIdValue",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionIdValue",
                table: "Chat_Message");

            migrationBuilder.DropTable(
                name: "Chat_SessionMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionIdValue",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "SessionIdValue",
                table: "Chat_Message");

            migrationBuilder.AddColumn<int>(
                name: "Badge",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationObjectType",
                table: "Chat_Session",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "MessageAutoId",
                table: "Chat_Session",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "MessageId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShowTime",
                table: "Chat_Session",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "Sorting",
                table: "Chat_Session",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_DestinationId",
                table: "Chat_Session",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_DestinationObjectType",
                table: "Chat_Session",
                column: "DestinationObjectType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_MessageId",
                table: "Chat_Session",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_DestinationId",
                table: "Chat_Session",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                table: "Chat_Session",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_Message_MessageId",
                table: "Chat_Session",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }
    }
}
