using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ChatUser_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MaxMessageAutoId",
                table: "Chat_ChatObject",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Chat_ChatUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ChatUser_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ChatUser");

            migrationBuilder.DropColumn(
                name: "MaxMessageAutoId",
                table: "Chat_ChatObject");
        }
    }
}
