using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntity_IMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "Chat_SessionUnitCounter",
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
                table: "Chat_Scoped",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ReadedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_OpenedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_MessageReminder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_HistoryMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_FriendshipTagUnit",
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
                table: "Chat_ChatObjectCategoryUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ArticleMessage",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionPermissionUnitGrant");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_SessionPermissionRoleGrant");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Scoped");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_MessageReminder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_FriendshipTagUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Follow");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_FavoritedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ChatObjectCategoryUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ArticleMessage");
        }
    }
}
