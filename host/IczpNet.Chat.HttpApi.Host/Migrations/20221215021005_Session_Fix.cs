using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Session_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Chat_Session",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "MessageChannel",
                table: "Chat_Session",
                newName: "DestinationObjectType");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Session_MessageChannel",
                table: "Chat_Session",
                newName: "IX_Chat_Session_DestinationObjectType");

            migrationBuilder.AddColumn<int>(
                name: "Badge",
                table: "Chat_Session",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MessageAutoId",
                table: "Chat_Session",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "MessageId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShowTime",
                table: "Chat_Session",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "Sorting",
                table: "Chat_Session",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Chat_Session",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_DestinationId",
                table: "Chat_Session",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_MessageId",
                table: "Chat_Session",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_DestinationId",
                table: "Chat_Session",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                table: "Chat_Session",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Session_Chat_Message_MessageId",
                table: "Chat_Session",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_DestinationId",
                table: "Chat_Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_Message_MessageId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_DestinationId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_MessageId",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "Badge",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "MessageAutoId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "ShowTime",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Chat_Session");

            migrationBuilder.RenameColumn(
                name: "DestinationObjectType",
                table: "Chat_Session",
                newName: "MessageChannel");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Chat_Session",
                newName: "Value");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Session_DestinationObjectType",
                table: "Chat_Session",
                newName: "IX_Chat_Session_MessageChannel");
        }
    }
}
