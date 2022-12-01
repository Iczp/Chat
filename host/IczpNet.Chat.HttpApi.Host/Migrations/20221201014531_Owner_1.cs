using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Owner_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficalExcludedMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_OfficalExcludedMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroupMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_OfficialGroupMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_OfficialMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting");

            migrationBuilder.RenameColumn(
                name: "ChatObjectId",
                table: "Chat_OfficialMember",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OfficialMember_ChatObjectId",
                table: "Chat_OfficialMember",
                newName: "IX_Chat_OfficialMember_OwnerId");

            migrationBuilder.RenameColumn(
                name: "ChatObjectId",
                table: "Chat_OfficialGroupMember",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OfficialGroupMember_ChatObjectId",
                table: "Chat_OfficialGroupMember",
                newName: "IX_Chat_OfficialGroupMember_OwnerId");

            migrationBuilder.RenameColumn(
                name: "ChatObjectId",
                table: "Chat_OfficalExcludedMember",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OfficalExcludedMember_ChatObjectId",
                table: "Chat_OfficalExcludedMember",
                newName: "IX_Chat_OfficalExcludedMember_OwnerId");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Chat_SessionSetting",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficalExcludedMember_Chat_ChatObject_OwnerId",
                table: "Chat_OfficalExcludedMember",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroupMember_Chat_ChatObject_OwnerId",
                table: "Chat_OfficialGroupMember",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialMember_Chat_ChatObject_OwnerId",
                table: "Chat_OfficialMember",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficalExcludedMember_Chat_ChatObject_OwnerId",
                table: "Chat_OfficalExcludedMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroupMember_Chat_ChatObject_OwnerId",
                table: "Chat_OfficialGroupMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialMember_Chat_ChatObject_OwnerId",
                table: "Chat_OfficialMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_OfficialMember",
                newName: "ChatObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OfficialMember_OwnerId",
                table: "Chat_OfficialMember",
                newName: "IX_Chat_OfficialMember_ChatObjectId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_OfficialGroupMember",
                newName: "ChatObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OfficialGroupMember_OwnerId",
                table: "Chat_OfficialGroupMember",
                newName: "IX_Chat_OfficialGroupMember_ChatObjectId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Chat_OfficalExcludedMember",
                newName: "ChatObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OfficalExcludedMember_OwnerId",
                table: "Chat_OfficalExcludedMember",
                newName: "IX_Chat_OfficalExcludedMember_ChatObjectId");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Chat_SessionSetting",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficalExcludedMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_OfficalExcludedMember",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroupMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_OfficialGroupMember",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialMember_Chat_ChatObject_ChatObjectId",
                table: "Chat_OfficialMember",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
