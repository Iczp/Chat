using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionOrganizationRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionOrganizationRole",
                columns: table => new
                {
                    SessionOrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    SessionRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionOrganizationRole", x => new { x.SessionOrganizationId, x.SessionRoleId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionOrganizationRole_Chat_SessionOrganization_SessionOrganizationId",
                        column: x => x.SessionOrganizationId,
                        principalTable: "Chat_SessionOrganization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionOrganizationRole_Chat_SessionRole_SessionRoleId",
                        column: x => x.SessionRoleId,
                        principalTable: "Chat_SessionRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionOrganizationRole_SessionRoleId",
                table: "Chat_SessionOrganizationRole",
                column: "SessionRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionOrganizationRole");
        }
    }
}
