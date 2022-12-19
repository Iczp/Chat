using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddProp_Id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnit");
        }
    }
}
