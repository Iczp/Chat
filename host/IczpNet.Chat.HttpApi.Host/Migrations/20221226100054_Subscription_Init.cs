using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Subscription_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_Subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Subscription_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Subscription_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Subscription_SessionId",
                table: "Chat_Subscription",
                column: "SessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_Subscription");
        }
    }
}
