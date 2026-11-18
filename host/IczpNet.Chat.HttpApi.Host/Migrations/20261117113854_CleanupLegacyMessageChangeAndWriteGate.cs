using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLegacyMessageChangeAndWriteGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_MessageChangeWriteGate");

            migrationBuilder.DropTable(
                name: "Chat_SessionMessageChangeState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_MessageChangeWriteGate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Epoch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastCommittedChangeId = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageChangeWriteGate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionMessageChangeState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Epoch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinVersion = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionMessageChangeState", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Chat_MessageChangeWriteGate",
                columns: new[] { "Id", "Epoch", "LastCommittedChangeId", "Revision" },
                values: new object[] { 1, new Guid("11111111-1111-1111-1111-111111111111"), 0L, 0L });
        }
    }
}
