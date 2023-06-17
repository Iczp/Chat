using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Wallet_Fix_Comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "WalletId",
                table: "Chat_WalletRecorder",
                type: "uniqueidentifier",
                nullable: true,
                comment: "钱包Id",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletBusinessId",
                table: "Chat_WalletRecorder",
                type: "nvarchar(450)",
                nullable: true,
                comment: "钱包业务Id",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "变化前-总金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "总金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LockAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "变化前-冻结金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LockAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "冻结金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_WalletRecorder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "说明",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "变化前-可用金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "可用金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                comment: "变更金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                comment: "总金额=可用金额+冻结金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Chat_Wallet",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "密码",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LockAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                comment: "冻结金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                comment: "可用金额",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "WalletId",
                table: "Chat_WalletRecorder",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                oldComment: "钱包Id");

            migrationBuilder.AlterColumn<string>(
                name: "WalletBusinessId",
                table: "Chat_WalletRecorder",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true,
                oldComment: "钱包业务Id");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "变化前-总金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "总金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "LockAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "变化前-冻结金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "LockAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "冻结金额");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_WalletRecorder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "说明");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableAmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "变化前-可用金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableAmount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "可用金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "变更金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "总金额=可用金额+冻结金额");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Chat_Wallet",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "密码");

            migrationBuilder.AlterColumn<decimal>(
                name: "LockAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "冻结金额");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "可用金额");
        }
    }
}
