using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionRequest_AddIndex_CreationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_CreationTime",
                table: "Chat_SessionRequest",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_CreationTime_Desc",
                table: "Chat_SessionRequest",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_HandleTime",
                table: "Chat_SessionRequest",
                column: "HandleTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_HandleTime_Desc",
                table: "Chat_SessionRequest",
                column: "HandleTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_IsAgreed",
                table: "Chat_SessionRequest",
                column: "IsAgreed");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_IsHandled",
                table: "Chat_SessionRequest",
                column: "IsHandled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRequest_CreationTime",
                table: "Chat_SessionRequest");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRequest_CreationTime_Desc",
                table: "Chat_SessionRequest");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRequest_HandleTime",
                table: "Chat_SessionRequest");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRequest_HandleTime_Desc",
                table: "Chat_SessionRequest");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRequest_IsAgreed",
                table: "Chat_SessionRequest");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRequest_IsHandled",
                table: "Chat_SessionRequest");
        }
    }
}
