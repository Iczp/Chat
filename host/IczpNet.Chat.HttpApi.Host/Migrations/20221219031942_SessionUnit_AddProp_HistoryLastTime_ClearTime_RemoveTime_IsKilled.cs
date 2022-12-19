using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddProp_HistoryLastTime_ClearTime_RemoveTime_IsKilled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClearTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HistoryLastTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKilled",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemoveTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClearTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "HistoryLastTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsKilled",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "RemoveTime",
                table: "Chat_SessionUnit");
        }
    }
}
