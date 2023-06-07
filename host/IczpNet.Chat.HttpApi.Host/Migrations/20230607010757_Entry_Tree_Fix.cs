using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Entry_Tree_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chat_EntryName",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_EntryName",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "Chat_EntryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Depth",
                table: "Chat_EntryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FullPath",
                table: "Chat_EntryName",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullPathName",
                table: "Chat_EntryName",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Chat_EntryName",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Sorting",
                table: "Chat_EntryName",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryName_FullPath",
                table: "Chat_EntryName",
                column: "FullPath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_EntryName_ParentId",
                table: "Chat_EntryName",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_EntryName_Chat_EntryName_ParentId",
                table: "Chat_EntryName",
                column: "ParentId",
                principalTable: "Chat_EntryName",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_EntryName_Chat_EntryName_ParentId",
                table: "Chat_EntryName");

            migrationBuilder.DropIndex(
                name: "IX_Chat_EntryName_FullPath",
                table: "Chat_EntryName");

            migrationBuilder.DropIndex(
                name: "IX_Chat_EntryName_ParentId",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "Depth",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "FullPath",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "FullPathName",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "Chat_EntryName");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chat_EntryName",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_EntryName",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
