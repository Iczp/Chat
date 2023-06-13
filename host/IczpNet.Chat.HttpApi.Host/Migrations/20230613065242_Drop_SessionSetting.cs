using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Drop_SessionSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionSetting");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    BackgroundImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsContacts = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsShowMemberName = table.Column<bool>(type: "bit", nullable: false),
                    IsShowRead = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortingNumber = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionSetting_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_DestinationId",
                table: "Chat_SessionSetting",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId");
        }
    }
}
