using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_AddIndex_SessionUnitId_Time : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_ClearTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryFristTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryLastTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_ClearTime",
                table: "Chat_SessionUnitSetting",
                columns: new[] { "SessionUnitId", "ClearTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_HistoryFristTime",
                table: "Chat_SessionUnitSetting",
                columns: new[] { "SessionUnitId", "HistoryFristTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_HistoryLastTime",
                table: "Chat_SessionUnitSetting",
                columns: new[] { "SessionUnitId", "HistoryLastTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_ClearTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_HistoryFristTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_HistoryLastTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_ClearTime",
                table: "Chat_SessionUnitSetting",
                column: "ClearTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryFristTime",
                table: "Chat_SessionUnitSetting",
                column: "HistoryFristTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryLastTime",
                table: "Chat_SessionUnitSetting",
                column: "HistoryLastTime");
        }
    }
}
