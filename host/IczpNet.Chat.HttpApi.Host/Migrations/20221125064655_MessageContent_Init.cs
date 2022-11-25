using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class MessageContent_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_TextContent_TextMessageListId",
                table: "Chat_Message_TextContent");

            migrationBuilder.RenameColumn(
                name: "TextMessageListId",
                table: "Chat_Message_TextContent",
                newName: "TextContentListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_TextContent_TextMessageListId",
                table: "Chat_Message_TextContent",
                newName: "IX_Chat_Message_TextContent_TextContentListId");

            migrationBuilder.CreateTable(
                name: "Chat_ArticleContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticleType = table.Column<int>(type: "int", nullable: false),
                    EditorType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Author = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VisitsCount = table.Column<long>(type: "bigint", nullable: false),
                    OriginalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ArticleContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_CmdContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cmd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CmdContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ContactsContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    MediaType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Picture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ContactsContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_FileContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ContentLength = table.Column<long>(type: "bigint", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_FileContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_HistoryContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_HistoryContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_HtmlContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditorType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    OriginalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_HtmlContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ImageContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Orientation = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Size = table.Column<int>(type: "int", nullable: true),
                    Qrcode = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ImageContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_LinkContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IssuerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IssuerIcon = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_LinkContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_LocationContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_LocationContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_RedEnvelopeContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantMode = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RedEnvelopeContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SoundContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Time = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SoundContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_VideoContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Size = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ImageWidth = table.Column<int>(type: "int", nullable: true),
                    ImageHeight = table.Column<int>(type: "int", nullable: true),
                    ImageSize = table.Column<int>(type: "int", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_VideoContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_ArticleContent",
                columns: table => new
                {
                    ArticleContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_ArticleContent", x => new { x.ArticleContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_ArticleContent_Chat_ArticleContent_ArticleContentListId",
                        column: x => x.ArticleContentListId,
                        principalTable: "Chat_ArticleContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_ArticleContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_CmdContent",
                columns: table => new
                {
                    CmdContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_CmdContent", x => new { x.CmdContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_CmdContent_Chat_CmdContent_CmdContentListId",
                        column: x => x.CmdContentListId,
                        principalTable: "Chat_CmdContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_CmdContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_ContactsContent",
                columns: table => new
                {
                    ContactsContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_ContactsContent", x => new { x.ContactsContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_ContactsContent_Chat_ContactsContent_ContactsContentListId",
                        column: x => x.ContactsContentListId,
                        principalTable: "Chat_ContactsContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_ContactsContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_FileContent",
                columns: table => new
                {
                    FileContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_FileContent", x => new { x.FileContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_FileContent_Chat_FileContent_FileContentListId",
                        column: x => x.FileContentListId,
                        principalTable: "Chat_FileContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_FileContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_HistoryMessage",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistoryContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_Chat_HistoryMessage", x => new { x.MessageId, x.HistoryContentId });
                    table.ForeignKey(
                        name: "FK_Chat_HistoryMessage_Chat_HistoryContent_HistoryContentId",
                        column: x => x.HistoryContentId,
                        principalTable: "Chat_HistoryContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_HistoryMessage_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_HistoryContent",
                columns: table => new
                {
                    HistoryContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_HistoryContent", x => new { x.HistoryContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_HistoryContent_Chat_HistoryContent_HistoryContentListId",
                        column: x => x.HistoryContentListId,
                        principalTable: "Chat_HistoryContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_HistoryContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_HtmlContent",
                columns: table => new
                {
                    HtmlContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_HtmlContent", x => new { x.HtmlContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_HtmlContent_Chat_HtmlContent_HtmlContentListId",
                        column: x => x.HtmlContentListId,
                        principalTable: "Chat_HtmlContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_HtmlContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_ImageContent",
                columns: table => new
                {
                    ImageContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_ImageContent", x => new { x.ImageContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_ImageContent_Chat_ImageContent_ImageContentListId",
                        column: x => x.ImageContentListId,
                        principalTable: "Chat_ImageContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_ImageContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_LinkContent",
                columns: table => new
                {
                    LinkContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_LinkContent", x => new { x.LinkContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_LinkContent_Chat_LinkContent_LinkContentListId",
                        column: x => x.LinkContentListId,
                        principalTable: "Chat_LinkContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_LinkContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_LocationContent",
                columns: table => new
                {
                    LocationContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_LocationContent", x => new { x.LocationContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_LocationContent_Chat_LocationContent_LocationContentListId",
                        column: x => x.LocationContentListId,
                        principalTable: "Chat_LocationContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_LocationContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_RedEnvelopeContent",
                columns: table => new
                {
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RedEnvelopeContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_RedEnvelopeContent", x => new { x.MessageListId, x.RedEnvelopeContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_RedEnvelopeContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_RedEnvelopeContent_Chat_RedEnvelopeContent_RedEnvelopeContentListId",
                        column: x => x.RedEnvelopeContentListId,
                        principalTable: "Chat_RedEnvelopeContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_RedEnvelopeUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RedEnvelopeContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTop = table.Column<bool>(type: "bit", nullable: false),
                    IsOwned = table.Column<bool>(type: "bit", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RollbackTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_RedEnvelopeUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_RedEnvelopeUnit_Chat_RedEnvelopeContent_RedEnvelopeContentId",
                        column: x => x.RedEnvelopeContentId,
                        principalTable: "Chat_RedEnvelopeContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_SoundContent",
                columns: table => new
                {
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoundContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_SoundContent", x => new { x.MessageListId, x.SoundContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_SoundContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_SoundContent_Chat_SoundContent_SoundContentListId",
                        column: x => x.SoundContentListId,
                        principalTable: "Chat_SoundContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_VideoContent",
                columns: table => new
                {
                    MessageListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_VideoContent", x => new { x.MessageListId, x.VideoContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_VideoContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_VideoContent_Chat_VideoContent_VideoContentListId",
                        column: x => x.VideoContentListId,
                        principalTable: "Chat_VideoContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HistoryMessage_HistoryContentId",
                table: "Chat_HistoryMessage",
                column: "HistoryContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ArticleContent_MessageListId",
                table: "Chat_Message_ArticleContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_CmdContent_MessageListId",
                table: "Chat_Message_CmdContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ContactsContent_MessageListId",
                table: "Chat_Message_ContactsContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_FileContent_MessageListId",
                table: "Chat_Message_FileContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_HistoryContent_MessageListId",
                table: "Chat_Message_HistoryContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_HtmlContent_MessageListId",
                table: "Chat_Message_HtmlContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ImageContent_MessageListId",
                table: "Chat_Message_ImageContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_LinkContent_MessageListId",
                table: "Chat_Message_LinkContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_LocationContent_MessageListId",
                table: "Chat_Message_LocationContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_RedEnvelopeContent",
                column: "RedEnvelopeContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SoundContent_SoundContentListId",
                table: "Chat_Message_SoundContent",
                column: "SoundContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_VideoContent_VideoContentListId",
                table: "Chat_Message_VideoContent",
                column: "VideoContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RedEnvelopeUnit_RedEnvelopeContentId",
                table: "Chat_RedEnvelopeUnit",
                column: "RedEnvelopeContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_TextContent",
                column: "TextContentListId",
                principalTable: "Chat_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_TextContent_TextContentListId",
                table: "Chat_Message_TextContent");

            migrationBuilder.DropTable(
                name: "Chat_HistoryMessage");

            migrationBuilder.DropTable(
                name: "Chat_Message_ArticleContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_CmdContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_ContactsContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_FileContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_HistoryContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_HtmlContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_ImageContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_LinkContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_LocationContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_RedEnvelopeContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_SoundContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_VideoContent");

            migrationBuilder.DropTable(
                name: "Chat_RedEnvelopeUnit");

            migrationBuilder.DropTable(
                name: "Chat_ArticleContent");

            migrationBuilder.DropTable(
                name: "Chat_CmdContent");

            migrationBuilder.DropTable(
                name: "Chat_ContactsContent");

            migrationBuilder.DropTable(
                name: "Chat_FileContent");

            migrationBuilder.DropTable(
                name: "Chat_HistoryContent");

            migrationBuilder.DropTable(
                name: "Chat_HtmlContent");

            migrationBuilder.DropTable(
                name: "Chat_ImageContent");

            migrationBuilder.DropTable(
                name: "Chat_LinkContent");

            migrationBuilder.DropTable(
                name: "Chat_LocationContent");

            migrationBuilder.DropTable(
                name: "Chat_SoundContent");

            migrationBuilder.DropTable(
                name: "Chat_VideoContent");

            migrationBuilder.DropTable(
                name: "Chat_RedEnvelopeContent");

            migrationBuilder.RenameColumn(
                name: "TextContentListId",
                table: "Chat_Message_TextContent",
                newName: "TextMessageListId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_Message_TextContent_TextContentListId",
                table: "Chat_Message_TextContent",
                newName: "IX_Chat_Message_TextContent_TextMessageListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_TextContent_Chat_TextContent_TextMessageListId",
                table: "Chat_Message_TextContent",
                column: "TextMessageListId",
                principalTable: "Chat_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
