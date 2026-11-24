using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Args",
                table: "Chat_Menu",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClickCount",
                table: "Chat_Menu",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ClickUsersCount",
                table: "Chat_Menu",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Chat_Menu",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Event",
                table: "Chat_Menu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_TemplateContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateVersion = table.Column<int>(type: "int", nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    FallbackText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_TemplateContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_TemplateContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessageTemplateDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PublishedVersion = table.Column<int>(type: "int", nullable: true),
                    DraftJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageTemplateDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessageTemplateDefinition_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_TemplateContent",
                columns: table => new
                {
                    MessageListId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_TemplateContent", x => new { x.MessageListId, x.TemplateContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_TemplateContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_TemplateContent_Chat_Message_Template_TemplateContent_TemplateContentListId",
                        column: x => x.TemplateContentListId,
                        principalTable: "Chat_Message_Template_TemplateContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessageTemplateRevision",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    DefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefinitionHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageTemplateRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessageTemplateRevision_ChatMessageTemplateDefinition_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ChatMessageTemplateDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_TemplateContent_TemplateContentListId",
                table: "Chat_Message_MapTo_TemplateContent",
                column: "TemplateContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_TemplateContent_OwnerId",
                table: "Chat_Message_Template_TemplateContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_TemplateContent_TemplateId",
                table: "Chat_Message_Template_TemplateContent",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageTemplateDefinition_OwnerId",
                table: "ChatMessageTemplateDefinition",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageTemplateDefinition_TenantId_OwnerId",
                table: "ChatMessageTemplateDefinition",
                columns: new[] { "TenantId", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageTemplateDefinition_TenantId_Visibility_IsEnabled",
                table: "ChatMessageTemplateDefinition",
                columns: new[] { "TenantId", "Visibility", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageTemplateRevision_TemplateId_Version",
                table: "ChatMessageTemplateRevision",
                columns: new[] { "TemplateId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_TemplateContent");

            migrationBuilder.DropTable(
                name: "ChatMessageTemplateRevision");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_TemplateContent");

            migrationBuilder.DropTable(
                name: "ChatMessageTemplateDefinition");

            migrationBuilder.DropColumn(
                name: "Args",
                table: "Chat_Menu");

            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Chat_Menu");

            migrationBuilder.DropColumn(
                name: "ClickUsersCount",
                table: "Chat_Menu");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Chat_Menu");

            migrationBuilder.DropColumn(
                name: "Event",
                table: "Chat_Menu");
        }
    }
}
