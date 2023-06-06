using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Drop_ChatObjectTargetEntryValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ChatObjectTargetEntryValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ChatObjectTargetEntryValue",
                columns: table => new
                {
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    EntryValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatObjectTargetEntryValue", x => new { x.OwnerId, x.EntryValueId });
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectTargetEntryValue_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectTargetEntryValue_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectTargetEntryValue_Chat_EntryValue_EntryValueId",
                        column: x => x.EntryValueId,
                        principalTable: "Chat_EntryValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectTargetEntryValue_CreationTime",
                table: "Chat_ChatObjectTargetEntryValue",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectTargetEntryValue_DestinationId",
                table: "Chat_ChatObjectTargetEntryValue",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectTargetEntryValue_EntryValueId",
                table: "Chat_ChatObjectTargetEntryValue",
                column: "EntryValueId");
        }
    }
}
