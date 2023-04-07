using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitOrganization_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionOrganization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FullPathName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    Sorting = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionOrganization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionOrganization_Chat_SessionOrganization_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Chat_SessionOrganization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionOrganization_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitOrganization",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionOrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitOrganization", x => new { x.SessionUnitId, x.SessionOrganizationId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitOrganization_Chat_SessionOrganization_SessionOrganizationId",
                        column: x => x.SessionOrganizationId,
                        principalTable: "Chat_SessionOrganization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitOrganization_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionOrganization_ParentId",
                table: "Chat_SessionOrganization",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionOrganization_SessionId",
                table: "Chat_SessionOrganization",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitOrganization_SessionOrganizationId",
                table: "Chat_SessionUnitOrganization",
                column: "SessionOrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitOrganization");

            migrationBuilder.DropTable(
                name: "Chat_SessionOrganization");
        }
    }
}
