using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Word_Init_And_IContentEntity_AddProp_IsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_VideoContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_VideoContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_TextContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_TextContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_SoundContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_SoundContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_LocationContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_LocationContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_LinkContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_LinkContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_ImageContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_ImageContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_HtmlContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_HtmlContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_HistoryContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_HistoryContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_FileContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_FileContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_ContactsContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_ContactsContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_CmdContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_CmdContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Message_Template_ArticleContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Chat_Message_Template_ArticleContent",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Chat_Word",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsDirty = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Word", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_TextContentWord",
                columns: table => new
                {
                    TextContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_TextContentWord", x => new { x.TextContentId, x.WordId });
                    table.ForeignKey(
                        name: "FK_Chat_TextContentWord_Chat_Message_Template_TextContent_TextContentId",
                        column: x => x.TextContentId,
                        principalTable: "Chat_Message_Template_TextContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_TextContentWord_Chat_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Chat_Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_TextContentWord_WordId",
                table: "Chat_TextContentWord",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Word_Value",
                table: "Chat_Word",
                column: "Value",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_TextContentWord");

            migrationBuilder.DropTable(
                name: "Chat_Word");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Chat_Message_Template_ArticleContent");
        }
    }
}
