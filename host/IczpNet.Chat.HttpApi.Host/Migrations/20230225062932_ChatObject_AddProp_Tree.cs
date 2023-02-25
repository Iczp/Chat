using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ChatObject_AddProp_Tree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Depth",
                table: "Chat_ChatObject",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FullPath",
                table: "Chat_ChatObject",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullPathName",
                table: "Chat_ChatObject",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Chat_ChatObject",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Sorting",
                table: "Chat_ChatObject",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_ParentId",
                table: "Chat_ChatObject",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObject_ParentId",
                table: "Chat_ChatObject",
                column: "ParentId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObject_ParentId",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_ParentId",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "Depth",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "FullPath",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "FullPathName",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "Chat_ChatObject");
        }
    }
}
