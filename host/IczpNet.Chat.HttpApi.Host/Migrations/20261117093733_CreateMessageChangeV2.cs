using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class CreateMessageChangeV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MessageChangeId",
                table: "Chat_Message",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_MessageChange",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageChange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_MessageChangeCursor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastAppliedMessageChangeId = table.Column<long>(type: "bigint", nullable: false),
                    IsInitialized = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageChangeCursor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_MessageChangeWriteGate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    Epoch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastCommittedChangeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageChangeWriteGate", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Chat_MessageChangeWriteGate",
                columns: new[] { "Id", "Epoch", "LastCommittedChangeId", "Revision" },
                values: new object[] { 1, new Guid("11111111-1111-1111-1111-111111111111"), 0L, 0L });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MessageChangeId",
                table: "Chat_Message",
                column: "MessageChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageChange_SessionId_Id",
                table: "Chat_MessageChange",
                columns: new[] { "SessionId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageChangeCursor_TenantId_UserId_ChatObjectId_DeviceId",
                table: "Chat_MessageChangeCursor",
                columns: new[] { "TenantId", "UserId", "ChatObjectId", "DeviceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_MessageChange_MessageChangeId",
                table: "Chat_Message",
                column: "MessageChangeId",
                principalTable: "Chat_MessageChange",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_MessageChange_MessageChangeId",
                table: "Chat_Message");

            migrationBuilder.DropTable(
                name: "Chat_MessageChange");

            migrationBuilder.DropTable(
                name: "Chat_MessageChangeCursor");

            migrationBuilder.DropTable(
                name: "Chat_MessageChangeWriteGate");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_MessageChangeId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "MessageChangeId",
                table: "Chat_Message");
        }
    }
}
