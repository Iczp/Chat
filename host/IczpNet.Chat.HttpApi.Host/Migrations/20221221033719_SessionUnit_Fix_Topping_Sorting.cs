using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_Fix_Topping_Sorting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Topping",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<double>(
                name: "Sorting",
                table: "Chat_SessionUnit",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<DateTime>(
                name: "Topping",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);
        }
    }
}
