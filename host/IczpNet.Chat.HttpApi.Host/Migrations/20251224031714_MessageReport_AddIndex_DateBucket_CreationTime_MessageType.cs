using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageReport_AddIndex_DateBucket_CreationTime_MessageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportMonth_MessageType",
                table: "Chat_MessageReportMonth");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportHour_MessageType",
                table: "Chat_MessageReportHour");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportDay_MessageType",
                table: "Chat_MessageReportDay");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_Count",
                table: "Chat_MessageReportMonth",
                column: "Count",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_CreationTime",
                table: "Chat_MessageReportMonth",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_DateBucket",
                table: "Chat_MessageReportMonth",
                column: "DateBucket",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_MessageType",
                table: "Chat_MessageReportMonth",
                column: "MessageType",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_Count",
                table: "Chat_MessageReportHour",
                column: "Count",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_CreationTime",
                table: "Chat_MessageReportHour",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_DateBucket",
                table: "Chat_MessageReportHour",
                column: "DateBucket",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_MessageType",
                table: "Chat_MessageReportHour",
                column: "MessageType",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_Count",
                table: "Chat_MessageReportDay",
                column: "Count",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_CreationTime",
                table: "Chat_MessageReportDay",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_DateBucket",
                table: "Chat_MessageReportDay",
                column: "DateBucket",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_MessageType",
                table: "Chat_MessageReportDay",
                column: "MessageType",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportMonth_Count",
                table: "Chat_MessageReportMonth");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportMonth_CreationTime",
                table: "Chat_MessageReportMonth");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportMonth_DateBucket",
                table: "Chat_MessageReportMonth");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportMonth_MessageType",
                table: "Chat_MessageReportMonth");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportHour_Count",
                table: "Chat_MessageReportHour");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportHour_CreationTime",
                table: "Chat_MessageReportHour");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportHour_DateBucket",
                table: "Chat_MessageReportHour");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportHour_MessageType",
                table: "Chat_MessageReportHour");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportDay_Count",
                table: "Chat_MessageReportDay");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportDay_CreationTime",
                table: "Chat_MessageReportDay");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportDay_DateBucket",
                table: "Chat_MessageReportDay");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageReportDay_MessageType",
                table: "Chat_MessageReportDay");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportMonth_MessageType",
                table: "Chat_MessageReportMonth",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportHour_MessageType",
                table: "Chat_MessageReportHour",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReportDay_MessageType",
                table: "Chat_MessageReportDay",
                column: "MessageType");
        }
    }
}
