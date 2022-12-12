using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class WalletRequest_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_PaymentPlatform",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_Chat_PaymentPlatform", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_WalletRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 64, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletRecorderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletBusinessId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentPlatformId = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    Descripton = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPosting = table.Column<bool>(type: "bit", nullable: false),
                    PostDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_WalletRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_PaymentPlatform_PaymentPlatformId",
                        column: x => x.PaymentPlatformId,
                        principalTable: "Chat_PaymentPlatform",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_WalletBusiness_WalletBusinessId",
                        column: x => x.WalletBusinessId,
                        principalTable: "Chat_WalletBusiness",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_WalletRecorder_WalletRecorderId",
                        column: x => x.WalletRecorderId,
                        principalTable: "Chat_WalletRecorder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_OwnerId",
                table: "Chat_WalletRequest",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_PaymentPlatformId",
                table: "Chat_WalletRequest",
                column: "PaymentPlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_WalletBusinessId",
                table: "Chat_WalletRequest",
                column: "WalletBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_WalletRecorderId",
                table: "Chat_WalletRequest",
                column: "WalletRecorderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_WalletRequest");

            migrationBuilder.DropTable(
                name: "Chat_PaymentPlatform");
        }
    }
}
