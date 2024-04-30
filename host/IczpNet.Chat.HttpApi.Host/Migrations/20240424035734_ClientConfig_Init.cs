using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ClientConfig_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ClientConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
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
                    table.PrimaryKey("PK_Chat_ClientConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ClientConfig_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ClientConfig_Description",
                table: "Chat_ClientConfig",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ClientConfig_DeviceId_AppUserId_Key_Platform",
                table: "Chat_ClientConfig",
                columns: new[] { "DeviceId", "AppUserId", "Key", "Platform" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ClientConfig_OwnerId",
                table: "Chat_ClientConfig",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ClientConfig_Platform",
                table: "Chat_ClientConfig",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ClientConfig_Title",
                table: "Chat_ClientConfig",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ClientConfig");
        }
    }
}
