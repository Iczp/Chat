using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Wallet_AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecorderId",
                table: "Chat_WalletOrder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_AvailableAmount",
                table: "Chat_WalletRecorder",
                column: "AvailableAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_LockAmount",
                table: "Chat_WalletRecorder",
                column: "LockAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_TotalAmount",
                table: "Chat_WalletRecorder",
                column: "TotalAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessType",
                table: "Chat_WalletRecorder",
                column: "WalletBusinessType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_Amount",
                table: "Chat_WalletOrder",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_BusinessType",
                table: "Chat_WalletOrder",
                column: "BusinessType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_IsEnabled",
                table: "Chat_WalletOrder",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_IsExpired",
                table: "Chat_WalletOrder",
                column: "IsExpired");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_OrderNo",
                table: "Chat_WalletOrder",
                column: "OrderNo",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_RecorderId",
                table: "Chat_WalletOrder",
                column: "RecorderId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_Status",
                table: "Chat_WalletOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_AppUserId",
                table: "Chat_Wallet",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_AvailableAmount",
                table: "Chat_Wallet",
                column: "AvailableAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_IsEnabled",
                table: "Chat_Wallet",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_IsLocked",
                table: "Chat_Wallet",
                column: "IsLocked");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_LockAmount",
                table: "Chat_Wallet",
                column: "LockAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_TotalAmount",
                table: "Chat_Wallet",
                column: "TotalAmount");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_WalletOrder_Chat_WalletRecorder_RecorderId",
                table: "Chat_WalletOrder",
                column: "RecorderId",
                principalTable: "Chat_WalletRecorder",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_WalletOrder_Chat_WalletRecorder_RecorderId",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_AvailableAmount",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_LockAmount",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_TotalAmount",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessType",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_Amount",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_BusinessType",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_IsEnabled",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_IsExpired",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_OrderNo",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_RecorderId",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletOrder_Status",
                table: "Chat_WalletOrder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Wallet_AppUserId",
                table: "Chat_Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Wallet_AvailableAmount",
                table: "Chat_Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Wallet_IsEnabled",
                table: "Chat_Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Wallet_IsLocked",
                table: "Chat_Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Wallet_LockAmount",
                table: "Chat_Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Wallet_TotalAmount",
                table: "Chat_Wallet");

            migrationBuilder.DropColumn(
                name: "RecorderId",
                table: "Chat_WalletOrder");
        }
    }
}
