using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ShopKeeper_init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ShopKeeper",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ShopKeeper", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ShopKeeper_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_ShopWaiter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopKeeperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ShopWaiter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ShopWaiter_Chat_ChatObject_ChatObjectId",
                        column: x => x.ChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ShopWaiter_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ShopWaiter_Chat_ShopKeeper_ShopKeeperId",
                        column: x => x.ShopKeeperId,
                        principalTable: "Chat_ShopKeeper",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopWaiter_ChatObjectId",
                table: "Chat_ShopWaiter",
                column: "ChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopWaiter_ShopKeeperId",
                table: "Chat_ShopWaiter",
                column: "ShopKeeperId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ShopWaiter");

            migrationBuilder.DropTable(
                name: "Chat_ShopKeeper");
        }
    }
}
