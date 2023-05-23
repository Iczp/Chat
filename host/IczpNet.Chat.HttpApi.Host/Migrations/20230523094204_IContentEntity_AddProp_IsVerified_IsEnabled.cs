using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class IContentEntity_AddProp_IsVerified_IsEnabled : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
