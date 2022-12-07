using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class ContactsContent_AddProp_DestinationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestinationId",
                table: "Chat_Message_Template_ContactsContent",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ContactsContent_DestinationId",
                table: "Chat_Message_Template_ContactsContent",
                column: "DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_DestinationId",
                table: "Chat_Message_Template_ContactsContent",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_DestinationId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ContactsContent_DestinationId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_Message_Template_ContactsContent");
        }
    }
}
