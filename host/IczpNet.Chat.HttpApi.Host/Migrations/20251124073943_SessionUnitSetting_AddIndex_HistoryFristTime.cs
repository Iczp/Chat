using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_AddIndex_HistoryFristTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_ClearTime",
                table: "Chat_SessionUnitSetting",
                column: "ClearTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryFristTime",
                table: "Chat_SessionUnitSetting",
                column: "HistoryFristTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryFristTime_HistoryLastTime_ClearTime",
                table: "Chat_SessionUnitSetting",
                columns: new[] { "HistoryFristTime", "HistoryLastTime", "ClearTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryLastTime",
                table: "Chat_SessionUnitSetting",
                column: "HistoryLastTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_IsEnabled_IsKilled",
                table: "Chat_SessionUnitSetting",
                columns: new[] { "SessionUnitId", "IsEnabled", "IsKilled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_ClearTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryFristTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryFristTime_HistoryLastTime_ClearTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_HistoryLastTime",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitSetting_SessionUnitId_IsEnabled_IsKilled",
                table: "Chat_SessionUnitSetting");
        }
    }
}
