using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Wallet_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorChatObjectId",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "Chat_RedEnvelopeUnit",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "OwnerTime",
                table: "Chat_RedEnvelopeUnit",
                newName: "OwnedTime");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.CreateTable(
                name: "Chat_Wallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_Chat_Wallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Wallet_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_WalletRecorder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_Chat_WalletRecorder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_WalletRecorder_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRecorder_Chat_Wallet_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RedEnvelopeUnit_OwnerId",
                table: "Chat_RedEnvelopeUnit",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_OwnerId",
                table: "Chat_Wallet",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_OwnerId",
                table: "Chat_WalletRecorder",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RedEnvelopeUnit_Chat_ChatObject_OwnerId",
                table: "Chat_RedEnvelopeUnit",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RedEnvelopeUnit_Chat_ChatObject_OwnerId",
                table: "Chat_RedEnvelopeUnit");

            migrationBuilder.DropTable(
                name: "Chat_WalletRecorder");

            migrationBuilder.DropTable(
                name: "Chat_Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Chat_RedEnvelopeUnit_OwnerId",
                table: "Chat_RedEnvelopeUnit");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_RedEnvelopeUnit",
                newName: "OwnerUserId");

            migrationBuilder.RenameColumn(
                name: "OwnedTime",
                table: "Chat_RedEnvelopeUnit",
                newName: "OwnerTime");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorChatObjectId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
