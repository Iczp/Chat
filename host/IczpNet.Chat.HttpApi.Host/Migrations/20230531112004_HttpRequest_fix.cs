using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class HttpRequest_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Chat_HttpRequest",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Chat_HttpRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Chat_HttpRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_HttpRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_HttpRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Chat_HttpRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_HttpRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Chat_HttpRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Chat_HttpRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_HttpRequest",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_HttpRequest");
        }
    }
}
