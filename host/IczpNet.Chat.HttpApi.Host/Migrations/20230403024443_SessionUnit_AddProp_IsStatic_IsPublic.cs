﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_IsStatic_IsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStatic",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsStatic",
                table: "Chat_SessionUnit");
        }
    }
}
