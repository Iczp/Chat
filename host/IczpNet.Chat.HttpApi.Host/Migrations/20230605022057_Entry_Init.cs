using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Entry_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_EntryName",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Chat_EntryName", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_EntryValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Chat_EntryValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_EntryValue_Chat_EntryName_EntryNameId",
                        column: x => x.EntryNameId,
                        principalTable: "Chat_EntryName",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ChatObjectEntryValue",
                columns: table => new
                {
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    EntryValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatObjectEntryValue", x => new { x.OwnerId, x.EntryValueId });
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectEntryValue_Chat_EntryValue_EntryValueId",
                        column: x => x.EntryValueId,
                        principalTable: "Chat_EntryValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitEntryValue",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitEntryValue", x => new { x.SessionUnitId, x.EntryValueId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitEntryValue_Chat_EntryValue_EntryValueId",
                        column: x => x.EntryValueId,
                        principalTable: "Chat_EntryValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitEntryValue_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectEntryValue_CreationTime",
                table: "Chat_ChatObjectEntryValue",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectEntryValue_EntryValueId",
                table: "Chat_ChatObjectEntryValue",
                column: "EntryValueId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryName_Code",
                table: "Chat_EntryName",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryName_IsStatic",
                table: "Chat_EntryName",
                column: "IsStatic");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryName_Name",
                table: "Chat_EntryName",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryValue_CreationTime",
                table: "Chat_EntryValue",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryValue_EntryNameId",
                table: "Chat_EntryValue",
                column: "EntryNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryValue_Value",
                table: "Chat_EntryValue",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitEntryValue_CreationTime",
                table: "Chat_SessionUnitEntryValue",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitEntryValue_EntryValueId",
                table: "Chat_SessionUnitEntryValue",
                column: "EntryValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ChatObjectEntryValue");

            migrationBuilder.DropTable(
                name: "Chat_SessionUnitEntryValue");

            migrationBuilder.DropTable(
                name: "Chat_EntryValue");

            migrationBuilder.DropTable(
                name: "Chat_EntryName");
        }
    }
}
