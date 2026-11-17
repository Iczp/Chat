using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyMessageChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageChange");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_MessageChange",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    MessageCreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiverSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageChange", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageChange_ChangedAt",
                table: "Chat_MessageChange",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageChange_SessionId_Version",
                table: "Chat_MessageChange",
                columns: new[] { "SessionId", "Version" },
                unique: true);
        }
    }
}
