using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class MessageContent_AddProp_OwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_VideoContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_TextContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_SoundContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_LocationContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_LinkContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_ImageContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_HtmlContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_HistoryContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_FileContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_CmdContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_Message_Template_ArticleContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_VideoContent_OwnerId",
                table: "Chat_Message_Template_VideoContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_TextContent_OwnerId",
                table: "Chat_Message_Template_TextContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_SoundContent_OwnerId",
                table: "Chat_Message_Template_SoundContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_RedEnvelopeContent_OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_LocationContent_OwnerId",
                table: "Chat_Message_Template_LocationContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_LinkContent_OwnerId",
                table: "Chat_Message_Template_LinkContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ImageContent_OwnerId",
                table: "Chat_Message_Template_ImageContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_HtmlContent_OwnerId",
                table: "Chat_Message_Template_HtmlContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_HistoryContent_OwnerId",
                table: "Chat_Message_Template_HistoryContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_FileContent_OwnerId",
                table: "Chat_Message_Template_FileContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_CmdContent_OwnerId",
                table: "Chat_Message_Template_CmdContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ArticleContent_OwnerId",
                table: "Chat_Message_Template_ArticleContent",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_ArticleContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_ArticleContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_CmdContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_CmdContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_FileContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_FileContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_HistoryContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_HistoryContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_HtmlContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_HtmlContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_ImageContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_ImageContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_LinkContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_LinkContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_LocationContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_LocationContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_RedEnvelopeContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_SoundContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_SoundContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_TextContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_TextContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_VideoContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_VideoContent",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_ArticleContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_CmdContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_FileContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_HistoryContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_HtmlContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_ImageContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_LinkContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_LocationContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_RedEnvelopeContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_SoundContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_TextContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_VideoContent_Chat_ChatObject_OwnerId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_VideoContent_OwnerId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_TextContent_OwnerId",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_SoundContent_OwnerId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_RedEnvelopeContent_OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_LocationContent_OwnerId",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_LinkContent_OwnerId",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ImageContent_OwnerId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_HtmlContent_OwnerId",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_HistoryContent_OwnerId",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_FileContent_OwnerId",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_CmdContent_OwnerId",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ArticleContent_OwnerId",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Message_Template_ArticleContent");
        }
    }
}
