using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddCallCenterQueueScanIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallCenterAssignment_Status_QueuedAt_Id_ShopKeeperId",
                table: "Chat_CallCenterAssignment",
                columns: new[] { "Status", "QueuedAt", "Id", "ShopKeeperId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_CallCenterAssignment_Status_QueuedAt_Id_ShopKeeperId",
                table: "Chat_CallCenterAssignment");
        }
    }
}
