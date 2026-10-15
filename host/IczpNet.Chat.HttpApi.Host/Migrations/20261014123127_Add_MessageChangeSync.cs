using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Add_MessageChangeSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_MessageChange",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageCreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    SenderSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverSessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageChange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionMessageChangeState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Epoch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    MinVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionMessageChangeState", x => x.Id);
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageChange");

            migrationBuilder.DropTable(
                name: "Chat_SessionMessageChangeState");
        }
    }
}
