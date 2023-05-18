using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class BaseRecord_Fix_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ReadedRecorder_SessionUnitId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_OpenedRecorder_SessionUnitId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_MessageReminder",
                table: "Chat_MessageReminder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReminder_SessionUnitId",
                table: "Chat_MessageReminder");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder",
                columns: new[] { "SessionUnitId", "MessageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder",
                columns: new[] { "SessionUnitId", "MessageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_MessageReminder",
                table: "Chat_MessageReminder",
                columns: new[] { "SessionUnitId", "MessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_MessageId",
                table: "Chat_ReadedRecorder",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_MessageId",
                table: "Chat_OpenedRecorder",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReminder_MessageId",
                table: "Chat_MessageReminder",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ReadedRecorder_MessageId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_OpenedRecorder_MessageId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_MessageReminder",
                table: "Chat_MessageReminder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReminder_MessageId",
                table: "Chat_MessageReminder");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder",
                columns: new[] { "MessageId", "SessionUnitId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder",
                columns: new[] { "MessageId", "SessionUnitId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_MessageReminder",
                table: "Chat_MessageReminder",
                columns: new[] { "MessageId", "SessionUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_SessionUnitId",
                table: "Chat_ReadedRecorder",
                column: "SessionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_SessionUnitId",
                table: "Chat_OpenedRecorder",
                column: "SessionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReminder_SessionUnitId",
                table: "Chat_MessageReminder",
                column: "SessionUnitId");
        }
    }
}
