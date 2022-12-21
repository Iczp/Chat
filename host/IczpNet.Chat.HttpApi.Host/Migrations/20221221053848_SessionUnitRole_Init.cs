using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnitRole_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Chat_SessionRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionRole_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitRole",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitRole", x => new { x.SessionUnitId, x.SessionRoleId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitRole_Chat_SessionRole_SessionRoleId",
                        column: x => x.SessionRoleId,
                        principalTable: "Chat_SessionRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitRole_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRole_SessionId",
                table: "Chat_SessionRole",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitRole_SessionRoleId",
                table: "Chat_SessionUnitRole",
                column: "SessionRoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitRole");

            migrationBuilder.DropTable(
                name: "Chat_SessionRole");
        }
    }
}
