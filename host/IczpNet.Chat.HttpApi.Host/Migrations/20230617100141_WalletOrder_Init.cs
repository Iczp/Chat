using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class WalletOrder_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_WalletOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "钱包Id"),
                    WalletBusinessId = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "钱包业务Id"),
                    WalletBusinessType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "标题"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "说明"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "变更金额"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "订单状态"),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "到期时间"),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false, comment: "是否到期"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "是否有效"),
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
                    table.PrimaryKey("PK_Chat_WalletOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_WalletOrder_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletOrder_Chat_WalletBusiness_WalletBusinessId",
                        column: x => x.WalletBusinessId,
                        principalTable: "Chat_WalletBusiness",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletOrder_Chat_Wallet_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Chat_Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_OwnerId",
                table: "Chat_WalletOrder",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_WalletBusinessId",
                table: "Chat_WalletOrder",
                column: "WalletBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletOrder_WalletId",
                table: "Chat_WalletOrder",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_WalletOrder");
        }
    }
}
