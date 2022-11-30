using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Enums_Add_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ChatObjectType",
                table: "Chat_ChatObject",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_MessageChannel",
                table: "Chat_Session",
                column: "MessageChannel");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_JoinWay",
                table: "Chat_RoomMember",
                column: "JoinWay");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_RoomRole",
                table: "Chat_RoomMember",
                column: "RoomRole");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_InvitationMethod",
                table: "Chat_Room",
                column: "InvitationMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_MemberNameDisplayMode",
                table: "Chat_Room",
                column: "MemberNameDisplayMode");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_PortraitDisplayMode",
                table: "Chat_Room",
                column: "PortraitDisplayMode");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_Type",
                table: "Chat_Room",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_RedEnvelopeContent_GrantMode",
                table: "Chat_Message_Template_RedEnvelopeContent",
                column: "GrantMode");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_HtmlContent_EditorType",
                table: "Chat_Message_Template_HtmlContent",
                column: "EditorType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ArticleContent_ArticleType",
                table: "Chat_Message_Template_ArticleContent",
                column: "ArticleType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ArticleContent_EditorType",
                table: "Chat_Message_Template_ArticleContent",
                column: "EditorType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MessageChannel",
                table: "Chat_Message",
                column: "MessageChannel");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MessageType",
                table: "Chat_Message",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_ChatObjectType",
                table: "Chat_ChatObject",
                column: "ChatObjectType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Session_MessageChannel",
                table: "Chat_Session");

            migrationBuilder.DropIndex(
                name: "IX_Chat_RoomMember_JoinWay",
                table: "Chat_RoomMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_RoomMember_RoomRole",
                table: "Chat_RoomMember");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_InvitationMethod",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_MemberNameDisplayMode",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_PortraitDisplayMode",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_Type",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_RedEnvelopeContent_GrantMode",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_HtmlContent_EditorType",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ArticleContent_ArticleType",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ArticleContent_EditorType",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_MessageChannel",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_MessageType",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_ChatObjectType",
                table: "Chat_ChatObject");

            migrationBuilder.AlterColumn<string>(
                name: "ChatObjectType",
                table: "Chat_ChatObject",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
