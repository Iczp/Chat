using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class WalletRecorder_RenameProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId",
                table: "Chat_WalletRecorder");

            migrationBuilder.RenameColumn(
                name: "WalletBusinessType",
                table: "Chat_WalletRecorder",
                newName: "BusinessType");

            migrationBuilder.RenameColumn(
                name: "WalletBusinessId",
                table: "Chat_WalletRecorder",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessType",
                table: "Chat_WalletRecorder",
                newName: "IX_Chat_WalletRecorder_BusinessType");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessId",
                table: "Chat_WalletRecorder",
                newName: "IX_Chat_WalletRecorder_BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_BusinessId",
                table: "Chat_WalletRecorder",
                column: "BusinessId",
                principalTable: "Chat_WalletBusiness",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_BusinessId",
                table: "Chat_WalletRecorder");

            migrationBuilder.RenameColumn(
                name: "BusinessType",
                table: "Chat_WalletRecorder",
                newName: "WalletBusinessType");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "Chat_WalletRecorder",
                newName: "WalletBusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_WalletRecorder_BusinessType",
                table: "Chat_WalletRecorder",
                newName: "IX_Chat_WalletRecorder_WalletBusinessType");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_WalletRecorder_BusinessId",
                table: "Chat_WalletRecorder",
                newName: "IX_Chat_WalletRecorder_WalletBusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId",
                table: "Chat_WalletRecorder",
                column: "WalletBusinessId",
                principalTable: "Chat_WalletBusiness",
                principalColumn: "Id");
        }
    }
}
