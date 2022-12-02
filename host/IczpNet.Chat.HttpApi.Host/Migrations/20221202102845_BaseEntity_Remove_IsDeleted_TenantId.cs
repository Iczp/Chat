using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class BaseEntity_Remove_IsDeleted_TenantId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_RoomRoleRoomMember");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_RoomRoleRoomMember");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_RoomRoleRoomMember");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_RoomRoleRoomMember");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_RoomPermissionGrant");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_RoomPermissionGrant");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_RoomPermissionGrant");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_RoomPermissionGrant");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_HistoryMessage");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_FriendshipTagUnit");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_FriendshipTagUnit");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_FriendshipTagUnit");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_FriendshipTagUnit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_RoomRoleRoomMember",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_RoomRoleRoomMember",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_RoomRoleRoomMember",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_RoomRoleRoomMember",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_RoomPermissionGrant",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_RoomPermissionGrant",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_RoomPermissionGrant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_RoomPermissionGrant",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_HistoryMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_HistoryMessage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_HistoryMessage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_HistoryMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_FriendshipTagUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_FriendshipTagUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_FriendshipTagUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_FriendshipTagUnit",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
