using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class WalletRecorder_Fix_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentAmount",
                table: "Chat_WalletRecorder",
                newName: "TotalAmountBeforeChange");

            migrationBuilder.RenameColumn(
                name: "AmountBeforeChange",
                table: "Chat_WalletRecorder",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LockAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LockAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableAmount",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "AvailableAmountBeforeChange",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "LockAmount",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "LockAmountBeforeChange",
                table: "Chat_WalletRecorder");

            migrationBuilder.RenameColumn(
                name: "TotalAmountBeforeChange",
                table: "Chat_WalletRecorder",
                newName: "CurrentAmount");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Chat_WalletRecorder",
                newName: "AmountBeforeChange");
        }
    }
}
