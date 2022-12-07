using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class OfficialGroup_Inherit_ChatObject_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_OfficialGroup");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_ChatObject_Id",
                table: "Chat_OfficialGroup",
                column: "Id",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_ChatObject_Id",
                table: "Chat_OfficialGroup");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Chat_OfficialGroup",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Chat_OfficialGroup",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_OfficialGroup",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Chat_OfficialGroup",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_OfficialGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Chat_OfficialGroup",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
