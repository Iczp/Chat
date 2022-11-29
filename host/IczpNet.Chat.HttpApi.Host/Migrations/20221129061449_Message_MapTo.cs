using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_MapTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                table: "Chat_Message_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_ArticleContent_Chat_Message_MessageListId",
                table: "Chat_Message_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_CmdContent_Chat_CmdContent_CmdContentListId",
                table: "Chat_Message_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_CmdContent_Chat_Message_MessageListId",
                table: "Chat_Message_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_ContactsContent_Chat_ContactsContent_ContactsContentListId",
                table: "Chat_Message_ContactsContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_ContactsContent_Chat_Message_MessageListId",
                table: "Chat_Message_ContactsContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_FileContent_Chat_FileContent_FileContentListId",
                table: "Chat_Message_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_FileContent_Chat_Message_MessageListId",
                table: "Chat_Message_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_HistoryContent_Chat_HistoryContent_HistoryContentListId",
                table: "Chat_Message_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_HistoryContent_Chat_Message_MessageListId",
                table: "Chat_Message_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_HtmlContent_Chat_HtmlContent_HtmlContentListId",
                table: "Chat_Message_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_HtmlContent_Chat_Message_MessageListId",
                table: "Chat_Message_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_ImageContent_Chat_ImageContent_ImageContentListId",
                table: "Chat_Message_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_ImageContent_Chat_Message_MessageListId",
                table: "Chat_Message_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_LinkContent_Chat_LinkContent_LinkContentListId",
                table: "Chat_Message_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_LinkContent_Chat_Message_MessageListId",
                table: "Chat_Message_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_LocationContent_Chat_LocationContent_LocationContentListId",
                table: "Chat_Message_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_LocationContent_Chat_Message_MessageListId",
                table: "Chat_Message_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_RedEnvelopeContent_Chat_Message_MessageListId",
                table: "Chat_Message_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_RedEnvelopeContent_Chat_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_SoundContent_Chat_Message_MessageListId",
                table: "Chat_Message_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_SoundContent_Chat_SoundContent_SoundContentListId",
                table: "Chat_Message_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_Message_MessageListId",
                table: "Chat_Message_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_VideoContent_Chat_Message_MessageListId",
                table: "Chat_Message_VideoContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_VideoContent_Chat_VideoContent_VideoContentListId",
                table: "Chat_Message_VideoContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_VideoContent",
                table: "Chat_Message_VideoContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_TextContent",
                table: "Chat_Message_TextContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_SoundContent",
                table: "Chat_Message_SoundContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_RedEnvelopeContent",
                table: "Chat_Message_RedEnvelopeContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_LocationContent",
                table: "Chat_Message_LocationContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_LinkContent",
                table: "Chat_Message_LinkContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_ImageContent",
                table: "Chat_Message_ImageContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_HtmlContent",
                table: "Chat_Message_HtmlContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_HistoryContent",
                table: "Chat_Message_HistoryContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_FileContent",
                table: "Chat_Message_FileContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_ContactsContent",
                table: "Chat_Message_ContactsContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_CmdContent",
                table: "Chat_Message_CmdContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_ArticleContent",
                table: "Chat_Message_ArticleContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_VideoContent",
                newName: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_TextContent",
                newName: "Chat_Message_MapTo_TextContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_SoundContent",
                newName: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_RedEnvelopeContent",
                newName: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_LocationContent",
                newName: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_LinkContent",
                newName: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_ImageContent",
                newName: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_HtmlContent",
                newName: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_HistoryContent",
                newName: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_FileContent",
                newName: "Chat_Message_MapTo_FileContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_ContactsContent",
                newName: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_CmdContent",
                newName: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_ArticleContent",
                newName: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent",
                newName: "IX_Chat_Message_MapTo_VideoContent_VideoContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent",
                newName: "IX_Chat_Message_MapTo_TextContent_TextContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent",
                newName: "IX_Chat_Message_MapTo_SoundContent_SoundContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent",
                newName: "IX_Chat_Message_MapTo_RedEnvelopeContent_RedEnvelopeContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_LocationContent_MessageListId",
                table: "Chat_Message_MapTo_LocationContent",
                newName: "IX_Chat_Message_MapTo_LocationContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_LinkContent_MessageListId",
                table: "Chat_Message_MapTo_LinkContent",
                newName: "IX_Chat_Message_MapTo_LinkContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_ImageContent_MessageListId",
                table: "Chat_Message_MapTo_ImageContent",
                newName: "IX_Chat_Message_MapTo_ImageContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_HtmlContent_MessageListId",
                table: "Chat_Message_MapTo_HtmlContent",
                newName: "IX_Chat_Message_MapTo_HtmlContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_HistoryContent_MessageListId",
                table: "Chat_Message_MapTo_HistoryContent",
                newName: "IX_Chat_Message_MapTo_HistoryContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_FileContent_MessageListId",
                table: "Chat_Message_MapTo_FileContent",
                newName: "IX_Chat_Message_MapTo_FileContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_ContactsContent_MessageListId",
                table: "Chat_Message_MapTo_ContactsContent",
                newName: "IX_Chat_Message_MapTo_ContactsContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_CmdContent_MessageListId",
                table: "Chat_Message_MapTo_CmdContent",
                newName: "IX_Chat_Message_MapTo_CmdContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_ArticleContent_MessageListId",
                table: "Chat_Message_MapTo_ArticleContent",
                newName: "IX_Chat_Message_MapTo_ArticleContent_MessageListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_VideoContent",
                table: "Chat_Message_MapTo_VideoContent",
                columns: new[] { "MessageListId", "VideoContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_TextContent",
                table: "Chat_Message_MapTo_TextContent",
                columns: new[] { "MessageListId", "TextContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_SoundContent",
                table: "Chat_Message_MapTo_SoundContent",
                columns: new[] { "MessageListId", "SoundContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_RedEnvelopeContent",
                table: "Chat_Message_MapTo_RedEnvelopeContent",
                columns: new[] { "MessageListId", "RedEnvelopeContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_LocationContent",
                table: "Chat_Message_MapTo_LocationContent",
                columns: new[] { "LocationContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_LinkContent",
                table: "Chat_Message_MapTo_LinkContent",
                columns: new[] { "LinkContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_ImageContent",
                table: "Chat_Message_MapTo_ImageContent",
                columns: new[] { "ImageContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_HtmlContent",
                table: "Chat_Message_MapTo_HtmlContent",
                columns: new[] { "HtmlContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_HistoryContent",
                table: "Chat_Message_MapTo_HistoryContent",
                columns: new[] { "HistoryContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_FileContent",
                table: "Chat_Message_MapTo_FileContent",
                columns: new[] { "FileContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_ContactsContent",
                table: "Chat_Message_MapTo_ContactsContent",
                columns: new[] { "ContactsContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_CmdContent",
                table: "Chat_Message_MapTo_CmdContent",
                columns: new[] { "CmdContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_MapTo_ArticleContent",
                table: "Chat_Message_MapTo_ArticleContent",
                columns: new[] { "ArticleContentListId", "MessageListId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                table: "Chat_Message_MapTo_ArticleContent",
                column: "ArticleContentListId",
                principalTable: "Chat_ArticleContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_ArticleContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_CmdContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_ContactsContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_FileContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_FileContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_HistoryContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_HtmlContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_ImageContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_LinkContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_LocationContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_SoundContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_TextContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_TextContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
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
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_VideoContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent",
                column: "VideoContentListId",
                principalTable: "Chat_VideoContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                table: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ArticleContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_CmdContent_CmdContentListId",
                table: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_CmdContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_ContactsContent_ContactsContentListId",
                table: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ContactsContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_FileContent_Chat_FileContent_FileContentListId",
                table: "Chat_Message_MapTo_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_FileContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_HistoryContent_HistoryContentListId",
                table: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HistoryContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_HtmlContent_HtmlContentListId",
                table: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_HtmlContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_ImageContent_ImageContentListId",
                table: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_ImageContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_LinkContent_LinkContentListId",
                table: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LinkContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_LocationContent_LocationContentListId",
                table: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_LocationContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_SoundContent_Chat_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_TextContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_Message_MessageListId",
                table: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_MapTo_VideoContent_Chat_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_VideoContent",
                table: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_TextContent",
                table: "Chat_Message_MapTo_TextContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_SoundContent",
                table: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_RedEnvelopeContent",
                table: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_LocationContent",
                table: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_LinkContent",
                table: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_ImageContent",
                table: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_HtmlContent",
                table: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_HistoryContent",
                table: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_FileContent",
                table: "Chat_Message_MapTo_FileContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_ContactsContent",
                table: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_CmdContent",
                table: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Message_MapTo_ArticleContent",
                table: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_VideoContent",
                newName: "Chat_Message_VideoContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_TextContent",
                newName: "Chat_Message_TextContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_SoundContent",
                newName: "Chat_Message_SoundContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_RedEnvelopeContent",
                newName: "Chat_Message_RedEnvelopeContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_LocationContent",
                newName: "Chat_Message_LocationContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_LinkContent",
                newName: "Chat_Message_LinkContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_ImageContent",
                newName: "Chat_Message_ImageContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_HtmlContent",
                newName: "Chat_Message_HtmlContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_HistoryContent",
                newName: "Chat_Message_HistoryContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_FileContent",
                newName: "Chat_Message_FileContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_ContactsContent",
                newName: "Chat_Message_ContactsContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_CmdContent",
                newName: "Chat_Message_CmdContent");

            migrationBuilder.RenameTable(
                name: "Chat_Message_MapTo_ArticleContent",
                newName: "Chat_Message_ArticleContent");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_VideoContent_VideoContentListId",
                table: "Chat_Message_VideoContent",
                newName: "IX_Chat_Message_VideoContent_VideoContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_TextContent_TextContentListId",
                table: "Chat_Message_TextContent",
                newName: "IX_Chat_Message_TextContent_TextContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_SoundContent_SoundContentListId",
                table: "Chat_Message_SoundContent",
                newName: "IX_Chat_Message_SoundContent_SoundContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_RedEnvelopeContent",
                newName: "IX_Chat_Message_RedEnvelopeContent_RedEnvelopeContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_LocationContent_MessageListId",
                table: "Chat_Message_LocationContent",
                newName: "IX_Chat_Message_LocationContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_LinkContent_MessageListId",
                table: "Chat_Message_LinkContent",
                newName: "IX_Chat_Message_LinkContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_ImageContent_MessageListId",
                table: "Chat_Message_ImageContent",
                newName: "IX_Chat_Message_ImageContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_HtmlContent_MessageListId",
                table: "Chat_Message_HtmlContent",
                newName: "IX_Chat_Message_HtmlContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_HistoryContent_MessageListId",
                table: "Chat_Message_HistoryContent",
                newName: "IX_Chat_Message_HistoryContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_FileContent_MessageListId",
                table: "Chat_Message_FileContent",
                newName: "IX_Chat_Message_FileContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_ContactsContent_MessageListId",
                table: "Chat_Message_ContactsContent",
                newName: "IX_Chat_Message_ContactsContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_CmdContent_MessageListId",
                table: "Chat_Message_CmdContent",
                newName: "IX_Chat_Message_CmdContent_MessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_MapTo_ArticleContent_MessageListId",
                table: "Chat_Message_ArticleContent",
                newName: "IX_Chat_Message_ArticleContent_MessageListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_VideoContent",
                table: "Chat_Message_VideoContent",
                columns: new[] { "MessageListId", "VideoContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_TextContent",
                table: "Chat_Message_TextContent",
                columns: new[] { "MessageListId", "TextContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_SoundContent",
                table: "Chat_Message_SoundContent",
                columns: new[] { "MessageListId", "SoundContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_RedEnvelopeContent",
                table: "Chat_Message_RedEnvelopeContent",
                columns: new[] { "MessageListId", "RedEnvelopeContentListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_LocationContent",
                table: "Chat_Message_LocationContent",
                columns: new[] { "LocationContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_LinkContent",
                table: "Chat_Message_LinkContent",
                columns: new[] { "LinkContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_ImageContent",
                table: "Chat_Message_ImageContent",
                columns: new[] { "ImageContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_HtmlContent",
                table: "Chat_Message_HtmlContent",
                columns: new[] { "HtmlContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_HistoryContent",
                table: "Chat_Message_HistoryContent",
                columns: new[] { "HistoryContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_FileContent",
                table: "Chat_Message_FileContent",
                columns: new[] { "FileContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_ContactsContent",
                table: "Chat_Message_ContactsContent",
                columns: new[] { "ContactsContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_CmdContent",
                table: "Chat_Message_CmdContent",
                columns: new[] { "CmdContentListId", "MessageListId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Message_ArticleContent",
                table: "Chat_Message_ArticleContent",
                columns: new[] { "ArticleContentListId", "MessageListId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                table: "Chat_Message_ArticleContent",
                column: "ArticleContentListId",
                principalTable: "Chat_ArticleContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_ArticleContent_Chat_Message_MessageListId",
                table: "Chat_Message_ArticleContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_CmdContent_Chat_CmdContent_CmdContentListId",
                table: "Chat_Message_CmdContent",
                column: "CmdContentListId",
                principalTable: "Chat_CmdContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_CmdContent_Chat_Message_MessageListId",
                table: "Chat_Message_CmdContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_ContactsContent_Chat_ContactsContent_ContactsContentListId",
                table: "Chat_Message_ContactsContent",
                column: "ContactsContentListId",
                principalTable: "Chat_ContactsContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_ContactsContent_Chat_Message_MessageListId",
                table: "Chat_Message_ContactsContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_FileContent_Chat_FileContent_FileContentListId",
                table: "Chat_Message_FileContent",
                column: "FileContentListId",
                principalTable: "Chat_FileContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_FileContent_Chat_Message_MessageListId",
                table: "Chat_Message_FileContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_HistoryContent_Chat_HistoryContent_HistoryContentListId",
                table: "Chat_Message_HistoryContent",
                column: "HistoryContentListId",
                principalTable: "Chat_HistoryContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_HistoryContent_Chat_Message_MessageListId",
                table: "Chat_Message_HistoryContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_HtmlContent_Chat_HtmlContent_HtmlContentListId",
                table: "Chat_Message_HtmlContent",
                column: "HtmlContentListId",
                principalTable: "Chat_HtmlContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_HtmlContent_Chat_Message_MessageListId",
                table: "Chat_Message_HtmlContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_ImageContent_Chat_ImageContent_ImageContentListId",
                table: "Chat_Message_ImageContent",
                column: "ImageContentListId",
                principalTable: "Chat_ImageContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_ImageContent_Chat_Message_MessageListId",
                table: "Chat_Message_ImageContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_LinkContent_Chat_LinkContent_LinkContentListId",
                table: "Chat_Message_LinkContent",
                column: "LinkContentListId",
                principalTable: "Chat_LinkContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_LinkContent_Chat_Message_MessageListId",
                table: "Chat_Message_LinkContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_LocationContent_Chat_LocationContent_LocationContentListId",
                table: "Chat_Message_LocationContent",
                column: "LocationContentListId",
                principalTable: "Chat_LocationContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_LocationContent_Chat_Message_MessageListId",
                table: "Chat_Message_LocationContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_RedEnvelopeContent_Chat_Message_MessageListId",
                table: "Chat_Message_RedEnvelopeContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_RedEnvelopeContent_Chat_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_RedEnvelopeContent",
                column: "RedEnvelopeContentListId",
                principalTable: "Chat_RedEnvelopeContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_SoundContent_Chat_Message_MessageListId",
                table: "Chat_Message_SoundContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_SoundContent_Chat_SoundContent_SoundContentListId",
                table: "Chat_Message_SoundContent",
                column: "SoundContentListId",
                principalTable: "Chat_SoundContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_Message_MessageListId",
                table: "Chat_Message_TextContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_TextContent",
                column: "TextContentListId",
                principalTable: "Chat_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_VideoContent_Chat_Message_MessageListId",
                table: "Chat_Message_VideoContent",
                column: "MessageListId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_VideoContent_Chat_VideoContent_VideoContentListId",
                table: "Chat_Message_VideoContent",
                column: "VideoContentListId",
                principalTable: "Chat_VideoContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
