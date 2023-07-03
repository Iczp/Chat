using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Remove_TenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted_TenantId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_TenantId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_TenantId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Word");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_WalletRequest");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_WalletRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_WalletOrder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_WalletBusiness");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Wallet");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_TextContentWord");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitTag");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitRole");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitOrganization");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitEntryValue");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnitContactTag");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionTag");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionRole");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionRequest");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionPermissionUnitGrant");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionPermissionRoleGrant");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionPermissionDefinition");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Scoped");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_RedEnvelopeUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_PaymentPlatform");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Motto");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_MessageReminder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_MessageContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_TextContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_HttpResponse");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Follow");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_FavoritedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_EntryValue");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Developer");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ContactTag");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Connection");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ChatObjectType");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ChatObjectEntryValue");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ChatObjectCategoryUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_BlobContent");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Blob");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ArticleMessage");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Article");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted" },
                descending: new[] { true, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted",
                table: "Chat_Message");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Word",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_WalletRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_WalletRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_WalletOrder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_WalletBusiness",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Wallet",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_TextContentWord",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitTag",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitSetting",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitRole",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitOrganization",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitEntryValue",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitCounter",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnitContactTag",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionTag",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionRole",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionPermissionUnitGrant",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionPermissionRoleGrant",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_SessionPermissionDefinition",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Scoped",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_RedEnvelopeUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ReadedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_PaymentPlatform",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_OpenedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Motto",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_MessageReminder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_MessageContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_VideoContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_TextContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_SoundContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_LocationContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_LinkContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_ImageContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_HtmlContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_HistoryContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_FileContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_ContactsContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_CmdContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message_Template_ArticleContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_HttpResponse",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_HttpRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_HistoryMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Follow",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_FavoritedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_EntryValue",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Developer",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ContactTag",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Connection",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ChatObjectType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ChatObjectEntryValue",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ChatObjectCategoryUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_BlobContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Blob",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ArticleMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Article",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted", "TenantId" },
                descending: new[] { true, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted_TenantId",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted", "TenantId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_TenantId",
                table: "Chat_SessionUnit",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_TenantId",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsPrivate", "SenderId", "ReceiverId", "IsDeleted", "TenantId" });
        }
    }
}
