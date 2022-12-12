using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class WalletRecorder_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_Wallet_OwnerId",
                table: "Chat_WalletRecorder");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_WalletId",
                table: "Chat_WalletRecorder",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_Wallet_WalletId",
                table: "Chat_WalletRecorder",
                column: "WalletId",
                principalTable: "Chat_Wallet",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_Wallet_WalletId",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_WalletId",
                table: "Chat_WalletRecorder");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_Wallet_OwnerId",
                table: "Chat_WalletRecorder",
                column: "OwnerId",
                principalTable: "Chat_Wallet",
                principalColumn: "Id");
        }
    }
}
