using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_Template : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_HistoryMessage_Chat_HistoryContent_HistoryContentId",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                table: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_CmdContent_CmdContentListId",
                table: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_ContactsContent_ContactsContentListId",
                table: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_FileContent_Chat_FileContent_FileContentListId",
                table: "Chat_Message_MapTo_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_HistoryContent_HistoryContentListId",
                table: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_HtmlContent_HtmlContentListId",
                table: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_ImageContent_ImageContentListId",
                table: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_LinkContent_LinkContentListId",
                table: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_LocationContent_LocationContentListId",
                table: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RedEnvelopeUnit_Chat_RedEnvelopeContent_RedEnvelopeContentId",
                table: "Chat_RedEnvelopeUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_VideoContent",
                table: "Chat_VideoContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_TextContent",
                table: "Chat_TextContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_SoundContent",
                table: "Chat_SoundContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_RedEnvelopeContent",
                table: "Chat_RedEnvelopeContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_LocationContent",
                table: "Chat_LocationContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_LinkContent",
                table: "Chat_LinkContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ImageContent",
                table: "Chat_ImageContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_HtmlContent",
                table: "Chat_HtmlContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_HistoryContent",
                table: "Chat_HistoryContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_FileContent",
                table: "Chat_FileContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ContactsContent",
                table: "Chat_ContactsContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_CmdContent",
                table: "Chat_CmdContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ArticleContent",
                table: "Chat_ArticleContent");

            migrationBuilder.RenameTable(
                name: "Chat_VideoContent",
                newName: "Chat_Message_Template_VideoContent");

            migrationBuilder.RenameTable(
                name: "Chat_TextContent",
                newName: "Chat_Message_Template_TextContent");

            migrationBuilder.RenameTable(
                name: "Chat_SoundContent",
                newName: "Chat_Message_Template_SoundContent");

            migrationBuilder.RenameTable(
                name: "Chat_RedEnvelopeContent",
                newName: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.RenameTable(
                name: "Chat_LocationContent",
                newName: "Chat_Message_Template_LocationContent");

            migrationBuilder.RenameTable(
                name: "Chat_LinkContent",
                newName: "Chat_Message_Template_LinkContent");

            migrationBuilder.RenameTable(
                name: "Chat_ImageContent",
                newName: "Chat_Message_Template_ImageContent");

            migrationBuilder.RenameTable(
                name: "Chat_HtmlContent",
                newName: "Chat_Message_Template_HtmlContent");

            migrationBuilder.RenameTable(
                name: "Chat_HistoryContent",
                newName: "Chat_Message_Template_HistoryContent");

            migrationBuilder.RenameTable(
                name: "Chat_FileContent",
                newName: "Chat_Message_Template_FileContent");

            migrationBuilder.RenameTable(
                name: "Chat_ContactsContent",
                newName: "Chat_Message_Template_ContactsContent");

            migrationBuilder.RenameTable(
                name: "Chat_CmdContent",
                newName: "Chat_Message_Template_CmdContent");

            migrationBuilder.RenameTable(
                name: "Chat_ArticleContent",
                newName: "Chat_Message_Template_ArticleContent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_VideoContent",
                table: "Chat_Message_Template_VideoContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_TextContent",
                table: "Chat_Message_Template_TextContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_SoundContent",
                table: "Chat_Message_Template_SoundContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_RedEnvelopeContent",
                table: "Chat_Message_Template_RedEnvelopeContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_LocationContent",
                table: "Chat_Message_Template_LocationContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_LinkContent",
                table: "Chat_Message_Template_LinkContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_ImageContent",
                table: "Chat_Message_Template_ImageContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_HtmlContent",
                table: "Chat_Message_Template_HtmlContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_HistoryContent",
                table: "Chat_Message_Template_HistoryContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_FileContent",
                table: "Chat_Message_Template_FileContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_ContactsContent",
                table: "Chat_Message_Template_ContactsContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_CmdContent",
                table: "Chat_Message_Template_CmdContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_Template_ArticleContent",
                table: "Chat_Message_Template_ArticleContent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HistoryMessage_Chat_Message_Template_HistoryContent_HistoryContentId",
                table: "Chat_HistoryMessage",
                column: "HistoryContentId",
                principalTable: "Chat_Message_Template_HistoryContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_Message_Template_ArticleContent_ArticleContentListId",
                table: "Chat_Message_MapTo_ArticleContent",
                column: "ArticleContentListId",
                principalTable: "Chat_Message_Template_ArticleContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_Message_Template_CmdContent_CmdContentListId",
                table: "Chat_Message_MapTo_CmdContent",
                column: "CmdContentListId",
                principalTable: "Chat_Message_Template_CmdContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_Message_Template_ContactsContent_ContactsContentListId",
                table: "Chat_Message_MapTo_ContactsContent",
                column: "ContactsContentListId",
                principalTable: "Chat_Message_Template_ContactsContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_FileContent_Chat_Message_Template_FileContent_FileContentListId",
                table: "Chat_Message_MapTo_FileContent",
                column: "FileContentListId",
                principalTable: "Chat_Message_Template_FileContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_Message_Template_HistoryContent_HistoryContentListId",
                table: "Chat_Message_MapTo_HistoryContent",
                column: "HistoryContentListId",
                principalTable: "Chat_Message_Template_HistoryContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_Message_Template_HtmlContent_HtmlContentListId",
                table: "Chat_Message_MapTo_HtmlContent",
                column: "HtmlContentListId",
                principalTable: "Chat_Message_Template_HtmlContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_Message_Template_ImageContent_ImageContentListId",
                table: "Chat_Message_MapTo_ImageContent",
                column: "ImageContentListId",
                principalTable: "Chat_Message_Template_ImageContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_Message_Template_LinkContent_LinkContentListId",
                table: "Chat_Message_MapTo_LinkContent",
                column: "LinkContentListId",
                principalTable: "Chat_Message_Template_LinkContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_Message_Template_LocationContent_LocationContentListId",
                table: "Chat_Message_MapTo_LocationContent",
                column: "LocationContentListId",
                principalTable: "Chat_Message_Template_LocationContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent",
                column: "RedEnvelopeContentListId",
                principalTable: "Chat_Message_Template_RedEnvelopeContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_Message_Template_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent",
                column: "SoundContentListId",
                principalTable: "Chat_Message_Template_SoundContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_TextContent_Chat_Message_Template_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent",
                column: "TextContentListId",
                principalTable: "Chat_Message_Template_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_Message_Template_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent",
                column: "VideoContentListId",
                principalTable: "Chat_Message_Template_VideoContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RedEnvelopeUnit_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentId",
                table: "Chat_RedEnvelopeUnit",
                column: "RedEnvelopeContentId",
                principalTable: "Chat_Message_Template_RedEnvelopeContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_HistoryMessage_Chat_Message_Template_HistoryContent_HistoryContentId",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_Message_Template_ArticleContent_ArticleContentListId",
                table: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_Message_Template_CmdContent_CmdContentListId",
                table: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_Message_Template_ContactsContent_ContactsContentListId",
                table: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_FileContent_Chat_Message_Template_FileContent_FileContentListId",
                table: "Chat_Message_MapTo_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_Message_Template_HistoryContent_HistoryContentListId",
                table: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_Message_Template_HtmlContent_HtmlContentListId",
                table: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_Message_Template_ImageContent_ImageContentListId",
                table: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_Message_Template_LinkContent_LinkContentListId",
                table: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_Message_Template_LocationContent_LocationContentListId",
                table: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_Message_Template_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_TextContent_Chat_Message_Template_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_Message_Template_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_RedEnvelopeUnit_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentId",
                table: "Chat_RedEnvelopeUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_VideoContent",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_TextContent",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_SoundContent",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_RedEnvelopeContent",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_LocationContent",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_LinkContent",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_ImageContent",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_HtmlContent",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_HistoryContent",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_FileContent",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_ContactsContent",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_CmdContent",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_Template_ArticleContent",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_VideoContent",
                newName: "Chat_VideoContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_TextContent",
                newName: "Chat_TextContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_SoundContent",
                newName: "Chat_SoundContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_RedEnvelopeContent",
                newName: "Chat_RedEnvelopeContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_LocationContent",
                newName: "Chat_LocationContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_LinkContent",
                newName: "Chat_LinkContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_ImageContent",
                newName: "Chat_ImageContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_HtmlContent",
                newName: "Chat_HtmlContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_HistoryContent",
                newName: "Chat_HistoryContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_FileContent",
                newName: "Chat_FileContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_ContactsContent",
                newName: "Chat_ContactsContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_CmdContent",
                newName: "Chat_CmdContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_Template_ArticleContent",
                newName: "Chat_ArticleContent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_VideoContent",
                table: "Chat_VideoContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_TextContent",
                table: "Chat_TextContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_SoundContent",
                table: "Chat_SoundContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_RedEnvelopeContent",
                table: "Chat_RedEnvelopeContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_LocationContent",
                table: "Chat_LocationContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_LinkContent",
                table: "Chat_LinkContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ImageContent",
                table: "Chat_ImageContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_HtmlContent",
                table: "Chat_HtmlContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_HistoryContent",
                table: "Chat_HistoryContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_FileContent",
                table: "Chat_FileContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ContactsContent",
                table: "Chat_ContactsContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_CmdContent",
                table: "Chat_CmdContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ArticleContent",
                table: "Chat_ArticleContent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HistoryMessage_Chat_HistoryContent_HistoryContentId",
                table: "Chat_HistoryMessage",
                column: "HistoryContentId",
                principalTable: "Chat_HistoryContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                table: "Chat_Message_MapTo_ArticleContent",
                column: "ArticleContentListId",
                principalTable: "Chat_ArticleContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_CmdContent_CmdContentListId",
                table: "Chat_Message_MapTo_CmdContent",
                column: "CmdContentListId",
                principalTable: "Chat_CmdContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_ContactsContent_ContactsContentListId",
                table: "Chat_Message_MapTo_ContactsContent",
                column: "ContactsContentListId",
                principalTable: "Chat_ContactsContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_FileContent_Chat_FileContent_FileContentListId",
                table: "Chat_Message_MapTo_FileContent",
                column: "FileContentListId",
                principalTable: "Chat_FileContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_HistoryContent_HistoryContentListId",
                table: "Chat_Message_MapTo_HistoryContent",
                column: "HistoryContentListId",
                principalTable: "Chat_HistoryContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_HtmlContent_HtmlContentListId",
                table: "Chat_Message_MapTo_HtmlContent",
                column: "HtmlContentListId",
                principalTable: "Chat_HtmlContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_ImageContent_ImageContentListId",
                table: "Chat_Message_MapTo_ImageContent",
                column: "ImageContentListId",
                principalTable: "Chat_ImageContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_LinkContent_LinkContentListId",
                table: "Chat_Message_MapTo_LinkContent",
                column: "LinkContentListId",
                principalTable: "Chat_LinkContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_LocationContent_LocationContentListId",
                table: "Chat_Message_MapTo_LocationContent",
                column: "LocationContentListId",
                principalTable: "Chat_LocationContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent",
                column: "RedEnvelopeContentListId",
                principalTable: "Chat_RedEnvelopeContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent",
                column: "SoundContentListId",
                principalTable: "Chat_SoundContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent",
                column: "TextContentListId",
                principalTable: "Chat_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent",
                column: "VideoContentListId",
                principalTable: "Chat_VideoContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_RedEnvelopeUnit_Chat_RedEnvelopeContent_RedEnvelopeContentId",
                table: "Chat_RedEnvelopeUnit",
                column: "RedEnvelopeContentId",
                principalTable: "Chat_RedEnvelopeContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
