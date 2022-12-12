using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class WalletBusiness_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Chat_Wallet",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountBeforeChange",
                table: "Chat_WalletRecorder",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_WalletRecorder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalletBusinessId",
                table: "Chat_WalletRecorder",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalletBusinessType",
                table: "Chat_WalletRecorder",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LockAmount",
                table: "Chat_Wallet",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Chat_WalletBusiness",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusinessType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_WalletBusiness", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessId",
                table: "Chat_WalletRecorder",
                column: "WalletBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessType",
                table: "Chat_WalletRecorder",
                column: "WalletBusinessType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletBusiness_BusinessType",
                table: "Chat_WalletBusiness",
                column: "BusinessType");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId",
                table: "Chat_WalletRecorder",
                column: "WalletBusinessId",
                principalTable: "Chat_WalletBusiness",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropTable(
                name: "Chat_WalletBusiness");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessId",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessType",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "AmountBeforeChange",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "WalletBusinessId",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "WalletBusinessType",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "AvailableAmount",
                table: "Chat_Wallet");

            migrationBuilder.DropColumn(
                name: "LockAmount",
                table: "Chat_Wallet");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Chat_Wallet",
                newName: "Amount");
        }
    }
}
