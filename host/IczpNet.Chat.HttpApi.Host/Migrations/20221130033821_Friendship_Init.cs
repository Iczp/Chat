using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Friendship_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_ChatObjectId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_Session_SessionId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_SessionSetting",
                table: "Chat_SessionSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionSetting_SessionId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "ChatObjectId",
                table: "Chat_SessionSetting");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Chat_SessionSetting",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationId",
                table: "Chat_SessionSetting",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Chat_SessionSetting",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_SessionSetting",
                table: "Chat_SessionSetting",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Chat_Friendship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FriendId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCantacts = table.Column<bool>(type: "bit", nullable: false),
                    SortingNumber = table.Column<long>(type: "bigint", nullable: true),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsShowMemberName = table.Column<bool>(type: "bit", nullable: false),
                    IsShowRead = table.Column<bool>(type: "bit", nullable: false),
                    BackgroundImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Chat_Friendship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Friendship_Chat_ChatObject_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Friendship_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_DestinationId",
                table: "Chat_SessionSetting",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Friendship_FriendId",
                table: "Chat_Friendship",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Friendship_OwnerId",
                table: "Chat_Friendship",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_DestinationId",
                table: "Chat_SessionSetting",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_DestinationId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropTable(
                name: "Chat_Friendship");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_SessionSetting",
                table: "Chat_SessionSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionSetting_DestinationId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionSetting_OwnerId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_SessionSetting");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Chat_SessionSetting",
                newName: "SessionId");

            migrationBuilder.AddColumn<Guid>(
                name: "ChatObjectId",
                table: "Chat_SessionSetting",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_SessionSetting",
                table: "Chat_SessionSetting",
                columns: new[] { "ChatObjectId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_SessionId",
                table: "Chat_SessionSetting",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_ChatObject_ChatObjectId",
                table: "Chat_SessionSetting",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionSetting_Chat_Session_SessionId",
                table: "Chat_SessionSetting",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
