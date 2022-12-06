using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class MessageContent_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectType",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_ContactsContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Portrait",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ContactsContent_ObjectType",
                table: "Chat_Message_Template_ContactsContent",
                column: "ObjectType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ContactsContent_OwnerId",
                table: "Chat_Message_Template_ContactsContent",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_ContactsContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ContactsContent_ObjectType",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ContactsContent_OwnerId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "ObjectType",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "Portrait",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaId",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Picture",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Chat_Message_Template_ContactsContent",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
