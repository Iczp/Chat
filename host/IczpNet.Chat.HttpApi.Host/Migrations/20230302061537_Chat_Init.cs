using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Chat_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_Article",
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
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
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
                    table.PrimaryKey("PK_Chat_Article", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ChatObjectType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxDepth = table.Column<int>(type: "int", nullable: false),
                    IsHasChild = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Chat_ChatObjectType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Connection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChatObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Server = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Agent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ActiveTime = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_Chat_Connection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_MessageContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
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
                    table.PrimaryKey("PK_Chat_MessageContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_PaymentPlatform",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Chat_PaymentPlatform", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_WalletBusiness",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusinessType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Chat_WalletBusiness", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ChatObject",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaxMessageAutoId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Portrait = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ObjectType = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ChatObjectTypeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FullPathName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    Sorting = table.Column<double>(type: "float", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatObject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ChatObject_Chat_ChatObjectType_ChatObjectTypeId",
                        column: x => x.ChatObjectTypeId,
                        principalTable: "Chat_ChatObjectType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ChatObject_Chat_ChatObject_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_ChatObjectCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatObjectTypeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FullPathName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    Sorting = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatObjectCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectCategory_Chat_ChatObjectCategory_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Chat_ChatObjectCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectCategory_Chat_ChatObjectType_ChatObjectTypeId",
                        column: x => x.ChatObjectTypeId,
                        principalTable: "Chat_ChatObjectType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Favorite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Chat_Favorite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Favorite_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_FriendshipRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsHandled = table.Column<bool>(type: "bit", nullable: false),
                    IsAgreed = table.Column<bool>(type: "bit", nullable: true),
                    HandlMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HandlTime = table.Column<DateTime>(type: "datetime2", maxLength: 200, nullable: true),
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_FriendshipRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_FriendshipRequest_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_FriendshipRequest_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_FriendshipTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Chat_FriendshipTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_FriendshipTag_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_ArticleContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_ArticleContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_ArticleContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_CmdContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cmd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_CmdContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_CmdContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_ContactsContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Portrait = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ObjectType = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_ContactsContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_FileContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_FileContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_FileContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_HistoryContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_HistoryContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_HistoryContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_HtmlContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_HtmlContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_HtmlContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_ImageContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_ImageContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_ImageContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_LinkContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_LinkContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_LinkContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_LocationContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_LocationContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_LocationContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_RedEnvelopeContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    GrantMode = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_Message_Template_RedEnvelopeContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_RedEnvelopeContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_SoundContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_SoundContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_SoundContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_TextContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_TextContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_TextContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_Template_VideoContent",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_Template_VideoContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Template_VideoContent_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCantacts = table.Column<bool>(type: "bit", nullable: false),
                    SortingNumber = table.Column<long>(type: "bigint", nullable: true),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsShowMemberName = table.Column<bool>(type: "bit", nullable: false),
                    IsShowRead = table.Column<bool>(type: "bit", nullable: false),
                    BackgroundImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionSetting_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionSetting_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Wallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    AvailableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LockAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Chat_Wallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Wallet_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ChatObjectCategoryUnit",
                columns: table => new
                {
                    ChatObjectId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatObjectCategoryUnit", x => new { x.ChatObjectId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectCategoryUnit_Chat_ChatObjectCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Chat_ChatObjectCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_ChatObjectCategoryUnit_Chat_ChatObject_ChatObjectId",
                        column: x => x.ChatObjectId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Friendship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCantacts = table.Column<bool>(type: "bit", nullable: false),
                    SortingNumber = table.Column<long>(type: "bigint", nullable: true),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsShowMemberName = table.Column<bool>(type: "bit", nullable: false),
                    IsShowRead = table.Column<bool>(type: "bit", nullable: false),
                    BackgroundImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPassive = table.Column<bool>(type: "bit", nullable: false),
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Friendship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Friendship_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Friendship_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Friendship_Chat_FriendshipRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Chat_FriendshipRequest",
                        principalColumn: "Id");
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    OwnedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                        name: "FK_Chat_RedEnvelopeUnit_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_RedEnvelopeUnit_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentId",
                        column: x => x.RedEnvelopeContentId,
                        principalTable: "Chat_Message_Template_RedEnvelopeContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_WalletRecorder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalletBusinessId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WalletBusinessType = table.Column<int>(type: "int", nullable: false),
                    AvailableAmountBeforeChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmountBeforeChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LockAmountBeforeChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LockAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Chat_WalletRecorder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_WalletRecorder_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId",
                        column: x => x.WalletBusinessId,
                        principalTable: "Chat_WalletBusiness",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRecorder_Chat_Wallet_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Chat_Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_FriendshipTagUnit",
                columns: table => new
                {
                    FriendshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FriendshipTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_FriendshipTagUnit", x => new { x.FriendshipId, x.FriendshipTagId });
                    table.ForeignKey(
                        name: "FK_Chat_FriendshipTagUnit_Chat_FriendshipTag_FriendshipTagId",
                        column: x => x.FriendshipTagId,
                        principalTable: "Chat_FriendshipTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_FriendshipTagUnit_Chat_Friendship_FriendshipId",
                        column: x => x.FriendshipId,
                        principalTable: "Chat_Friendship",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_WalletRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 64, nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    WalletRecorderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletBusinessId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentPlatformId = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    Descripton = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPosting = table.Column<bool>(type: "bit", nullable: false),
                    PostDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_WalletRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_PaymentPlatform_PaymentPlatformId",
                        column: x => x.PaymentPlatformId,
                        principalTable: "Chat_PaymentPlatform",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_WalletBusiness_WalletBusinessId",
                        column: x => x.WalletBusinessId,
                        principalTable: "Chat_WalletBusiness",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_WalletRequest_Chat_WalletRecorder_WalletRecorderId",
                        column: x => x.WalletRecorderId,
                        principalTable: "Chat_WalletRecorder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ArticleMessage",
                columns: table => new
                {
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ArticleMessage", x => new { x.MessageId, x.ArticleId });
                    table.ForeignKey(
                        name: "FK_Chat_ArticleMessage_Chat_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Chat_Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_FavoriteMessage",
                columns: table => new
                {
                    FavoriteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_FavoriteMessage", x => new { x.MessageId, x.FavoriteId });
                    table.ForeignKey(
                        name: "FK_Chat_FavoriteMessage_Chat_Favorite_FavoriteId",
                        column: x => x.FavoriteId,
                        principalTable: "Chat_Favorite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_HistoryMessage",
                columns: table => new
                {
                    HistoryContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_HistoryMessage", x => new { x.MessageId, x.HistoryContentId });
                    table.ForeignKey(
                        name: "FK_Chat_HistoryMessage_Chat_Message_Template_HistoryContent_HistoryContentId",
                        column: x => x.HistoryContentId,
                        principalTable: "Chat_Message_Template_HistoryContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutoId = table.Column<long>(type: "bigint", nullable: false),
                    SessionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionUnitCount = table.Column<int>(type: "int", nullable: false),
                    MessageContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SenderId = table.Column<long>(type: "bigint", nullable: true),
                    SenderType = table.Column<int>(type: "int", nullable: true),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: true),
                    ReceiverType = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    IsRemindAll = table.Column<bool>(type: "bit", nullable: false),
                    KeyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KeyValue = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    IsRollbacked = table.Column<bool>(type: "bit", nullable: false),
                    RollbackTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ForwardMessageId = table.Column<long>(type: "bigint", nullable: true),
                    ForwardDepth = table.Column<long>(type: "bigint", nullable: false),
                    ForwardPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QuoteMessageId = table.Column<long>(type: "bigint", nullable: true),
                    QuoteDepth = table.Column<long>(type: "bigint", nullable: false),
                    QuotePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_Chat_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_ChatObject_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_ChatObject_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_MessageContent_MessageContentId",
                        column: x => x.MessageContentId,
                        principalTable: "Chat_MessageContent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_Message_ForwardMessageId",
                        column: x => x.ForwardMessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Message_Chat_Message_QuoteMessageId",
                        column: x => x.QuoteMessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_ArticleContent",
                columns: table => new
                {
                    ArticleContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_ArticleContent", x => new { x.ArticleContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_ArticleContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_ArticleContent_Chat_Message_Template_ArticleContent_ArticleContentListId",
                        column: x => x.ArticleContentListId,
                        principalTable: "Chat_Message_Template_ArticleContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_CmdContent",
                columns: table => new
                {
                    CmdContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_CmdContent", x => new { x.CmdContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_CmdContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_CmdContent_Chat_Message_Template_CmdContent_CmdContentListId",
                        column: x => x.CmdContentListId,
                        principalTable: "Chat_Message_Template_CmdContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_ContactsContent",
                columns: table => new
                {
                    ContactsContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_ContactsContent", x => new { x.ContactsContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_ContactsContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_ContactsContent_Chat_Message_Template_ContactsContent_ContactsContentListId",
                        column: x => x.ContactsContentListId,
                        principalTable: "Chat_Message_Template_ContactsContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_FileContent",
                columns: table => new
                {
                    FileContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_FileContent", x => new { x.FileContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_FileContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_FileContent_Chat_Message_Template_FileContent_FileContentListId",
                        column: x => x.FileContentListId,
                        principalTable: "Chat_Message_Template_FileContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_HistoryContent",
                columns: table => new
                {
                    HistoryContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_HistoryContent", x => new { x.HistoryContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_HistoryContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_HistoryContent_Chat_Message_Template_HistoryContent_HistoryContentListId",
                        column: x => x.HistoryContentListId,
                        principalTable: "Chat_Message_Template_HistoryContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_HtmlContent",
                columns: table => new
                {
                    HtmlContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_HtmlContent", x => new { x.HtmlContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_HtmlContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_HtmlContent_Chat_Message_Template_HtmlContent_HtmlContentListId",
                        column: x => x.HtmlContentListId,
                        principalTable: "Chat_Message_Template_HtmlContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_ImageContent",
                columns: table => new
                {
                    ImageContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_ImageContent", x => new { x.ImageContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_ImageContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_ImageContent_Chat_Message_Template_ImageContent_ImageContentListId",
                        column: x => x.ImageContentListId,
                        principalTable: "Chat_Message_Template_ImageContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_LinkContent",
                columns: table => new
                {
                    LinkContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_LinkContent", x => new { x.LinkContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_LinkContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_LinkContent_Chat_Message_Template_LinkContent_LinkContentListId",
                        column: x => x.LinkContentListId,
                        principalTable: "Chat_Message_Template_LinkContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_LocationContent",
                columns: table => new
                {
                    LocationContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_LocationContent", x => new { x.LocationContentListId, x.MessageListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_LocationContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_LocationContent_Chat_Message_Template_LocationContent_LocationContentListId",
                        column: x => x.LocationContentListId,
                        principalTable: "Chat_Message_Template_LocationContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_RedEnvelopeContent",
                columns: table => new
                {
                    MessageListId = table.Column<long>(type: "bigint", nullable: false),
                    RedEnvelopeContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_RedEnvelopeContent", x => new { x.MessageListId, x.RedEnvelopeContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentListId",
                        column: x => x.RedEnvelopeContentListId,
                        principalTable: "Chat_Message_Template_RedEnvelopeContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_SoundContent",
                columns: table => new
                {
                    MessageListId = table.Column<long>(type: "bigint", nullable: false),
                    SoundContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_SoundContent", x => new { x.MessageListId, x.SoundContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_SoundContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_SoundContent_Chat_Message_Template_SoundContent_SoundContentListId",
                        column: x => x.SoundContentListId,
                        principalTable: "Chat_Message_Template_SoundContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_TextContent",
                columns: table => new
                {
                    MessageListId = table.Column<long>(type: "bigint", nullable: false),
                    TextContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_TextContent", x => new { x.MessageListId, x.TextContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_TextContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_TextContent_Chat_Message_Template_TextContent_TextContentListId",
                        column: x => x.TextContentListId,
                        principalTable: "Chat_Message_Template_TextContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Message_MapTo_VideoContent",
                columns: table => new
                {
                    MessageListId = table.Column<long>(type: "bigint", nullable: false),
                    VideoContentListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Message_MapTo_VideoContent", x => new { x.MessageListId, x.VideoContentListId });
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_VideoContent_Chat_Message_MessageListId",
                        column: x => x.MessageListId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Message_MapTo_VideoContent_Chat_Message_Template_VideoContent_VideoContentListId",
                        column: x => x.VideoContentListId,
                        principalTable: "Chat_Message_Template_VideoContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_OpenedRecorder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    MessageAutoId = table.Column<long>(type: "bigint", nullable: true),
                    MessageId = table.Column<long>(type: "bigint", nullable: true),
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OpenedRecorder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_OpenedRecorder_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_ReadedRecorder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    MessageAutoId = table.Column<long>(type: "bigint", nullable: true),
                    MessageId = table.Column<long>(type: "bigint", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ReadedRecorder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ReadedRecorder_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Session",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    LastMessageId = table.Column<long>(type: "bigint", nullable: true),
                    LastMessageAutoId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Chat_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Session_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Session_Chat_Message_LastMessageId",
                        column: x => x.LastMessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Chat_SessionRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionRole_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Chat_SessionTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionTag_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationObjectType = table.Column<int>(type: "int", nullable: true),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReadedMessageAutoId = table.Column<long>(type: "bigint", nullable: false),
                    LastMessageAutoId = table.Column<long>(type: "bigint", nullable: false),
                    HistoryFristTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HistoryLastTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsKilled = table.Column<bool>(type: "bit", nullable: false),
                    KillType = table.Column<int>(type: "int", nullable: true),
                    KillTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KillerId = table.Column<long>(type: "bigint", nullable: true),
                    ClearTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemoveTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false),
                    JoinWay = table.Column<int>(type: "int", nullable: true),
                    InviterId = table.Column<long>(type: "bigint", nullable: true),
                    Sorting = table.Column<double>(type: "float", nullable: false),
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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_ChatObject_InviterId",
                        column: x => x.InviterId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_ChatObject_KillerId",
                        column: x => x.KillerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnit_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_MessageReminder",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_MessageReminder", x => new { x.MessageId, x.SessionUnitId });
                    table.ForeignKey(
                        name: "FK_Chat_MessageReminder_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitRole",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitRole", x => new { x.SessionUnitId, x.SessionRoleId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitRole_Chat_SessionRole_SessionRoleId",
                        column: x => x.SessionRoleId,
                        principalTable: "Chat_SessionRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitRole_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitTag",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitTag", x => new { x.SessionUnitId, x.SessionTagId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitTag_Chat_SessionTag_SessionTagId",
                        column: x => x.SessionTagId,
                        principalTable: "Chat_SessionTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitTag_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ArticleMessage_ArticleId",
                table: "Chat_ArticleMessage",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_ChatObjectTypeId",
                table: "Chat_ChatObject",
                column: "ChatObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_ParentId",
                table: "Chat_ChatObject",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectCategory_ChatObjectTypeId",
                table: "Chat_ChatObjectCategory",
                column: "ChatObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectCategory_ParentId",
                table: "Chat_ChatObjectCategory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectCategoryUnit_CategoryId",
                table: "Chat_ChatObjectCategoryUnit",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Favorite_OwnerId",
                table: "Chat_Favorite",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FavoriteMessage_FavoriteId",
                table: "Chat_FavoriteMessage",
                column: "FavoriteId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Friendship_DestinationId",
                table: "Chat_Friendship",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Friendship_OwnerId",
                table: "Chat_Friendship",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Friendship_RequestId",
                table: "Chat_Friendship",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FriendshipRequest_DestinationId",
                table: "Chat_FriendshipRequest",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FriendshipRequest_OwnerId",
                table: "Chat_FriendshipRequest",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FriendshipTag_OwnerId",
                table: "Chat_FriendshipTag",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FriendshipTagUnit_FriendshipTagId",
                table: "Chat_FriendshipTagUnit",
                column: "FriendshipTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HistoryMessage_HistoryContentId",
                table: "Chat_HistoryMessage",
                column: "HistoryContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_AutoId",
                table: "Chat_Message",
                column: "AutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ForwardMessageId",
                table: "Chat_Message",
                column: "ForwardMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MessageContentId",
                table: "Chat_Message",
                column: "MessageContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_QuoteMessageId",
                table: "Chat_Message",
                column: "QuoteMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ReceiverId",
                table: "Chat_Message",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderId",
                table: "Chat_Message",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionId",
                table: "Chat_Message",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SessionUnitCount",
                table: "Chat_Message",
                column: "SessionUnitCount");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_ArticleContent_MessageListId",
                table: "Chat_Message_MapTo_ArticleContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_CmdContent_MessageListId",
                table: "Chat_Message_MapTo_CmdContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_ContactsContent_MessageListId",
                table: "Chat_Message_MapTo_ContactsContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_FileContent_MessageListId",
                table: "Chat_Message_MapTo_FileContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_HistoryContent_MessageListId",
                table: "Chat_Message_MapTo_HistoryContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_HtmlContent_MessageListId",
                table: "Chat_Message_MapTo_HtmlContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_ImageContent_MessageListId",
                table: "Chat_Message_MapTo_ImageContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_LinkContent_MessageListId",
                table: "Chat_Message_MapTo_LinkContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_LocationContent_MessageListId",
                table: "Chat_Message_MapTo_LocationContent",
                column: "MessageListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_RedEnvelopeContent_RedEnvelopeContentListId",
                table: "Chat_Message_MapTo_RedEnvelopeContent",
                column: "RedEnvelopeContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_SoundContent_SoundContentListId",
                table: "Chat_Message_MapTo_SoundContent",
                column: "SoundContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_TextContent_TextContentListId",
                table: "Chat_Message_MapTo_TextContent",
                column: "TextContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MapTo_VideoContent_VideoContentListId",
                table: "Chat_Message_MapTo_VideoContent",
                column: "VideoContentListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ArticleContent_OwnerId",
                table: "Chat_Message_Template_ArticleContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_CmdContent_OwnerId",
                table: "Chat_Message_Template_CmdContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ContactsContent_DestinationId",
                table: "Chat_Message_Template_ContactsContent",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ContactsContent_OwnerId",
                table: "Chat_Message_Template_ContactsContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_FileContent_OwnerId",
                table: "Chat_Message_Template_FileContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_HistoryContent_OwnerId",
                table: "Chat_Message_Template_HistoryContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_HtmlContent_OwnerId",
                table: "Chat_Message_Template_HtmlContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ImageContent_OwnerId",
                table: "Chat_Message_Template_ImageContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_LinkContent_OwnerId",
                table: "Chat_Message_Template_LinkContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_LocationContent_OwnerId",
                table: "Chat_Message_Template_LocationContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_RedEnvelopeContent_OwnerId",
                table: "Chat_Message_Template_RedEnvelopeContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_SoundContent_OwnerId",
                table: "Chat_Message_Template_SoundContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_TextContent_OwnerId",
                table: "Chat_Message_Template_TextContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_VideoContent_OwnerId",
                table: "Chat_Message_Template_VideoContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageReminder_SessionUnitId",
                table: "Chat_MessageReminder",
                column: "SessionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_DestinationId",
                table: "Chat_OpenedRecorder",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_MessageId",
                table: "Chat_OpenedRecorder",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_OwnerId",
                table: "Chat_OpenedRecorder",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_DestinationId",
                table: "Chat_ReadedRecorder",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_MessageId",
                table: "Chat_ReadedRecorder",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_OwnerId",
                table: "Chat_ReadedRecorder",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RedEnvelopeUnit_OwnerId",
                table: "Chat_RedEnvelopeUnit",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RedEnvelopeUnit_RedEnvelopeContentId",
                table: "Chat_RedEnvelopeUnit",
                column: "RedEnvelopeContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageAutoId",
                table: "Chat_Session",
                column: "LastMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_LastMessageId",
                table: "Chat_Session",
                column: "LastMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_OwnerId",
                table: "Chat_Session",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Session_SessionKey",
                table: "Chat_Session",
                column: "SessionKey");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRole_SessionId",
                table: "Chat_SessionRole",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_DestinationId",
                table: "Chat_SessionSetting",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionSetting_OwnerId",
                table: "Chat_SessionSetting",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionTag_SessionId",
                table: "Chat_SessionTag",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_DestinationId",
                table: "Chat_SessionUnit",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_InviterId",
                table: "Chat_SessionUnit",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_KillerId",
                table: "Chat_SessionUnit",
                column: "KillerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit",
                column: "LastMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_SessionId",
                table: "Chat_SessionUnit",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting",
                table: "Chat_SessionUnit",
                column: "Sorting",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitRole_SessionRoleId",
                table: "Chat_SessionUnitRole",
                column: "SessionRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitTag_SessionTagId",
                table: "Chat_SessionUnitTag",
                column: "SessionTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Wallet_OwnerId",
                table: "Chat_Wallet",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_OwnerId",
                table: "Chat_WalletRecorder",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_WalletBusinessId",
                table: "Chat_WalletRecorder",
                column: "WalletBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRecorder_WalletId",
                table: "Chat_WalletRecorder",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_OwnerId",
                table: "Chat_WalletRequest",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_PaymentPlatformId",
                table: "Chat_WalletRequest",
                column: "PaymentPlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_WalletBusinessId",
                table: "Chat_WalletRequest",
                column: "WalletBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_WalletRequest_WalletRecorderId",
                table: "Chat_WalletRequest",
                column: "WalletRecorderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ArticleMessage_Chat_Message_MessageId",
                table: "Chat_ArticleMessage",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_FavoriteMessage_Chat_Message_MessageId",
                table: "Chat_FavoriteMessage",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HistoryMessage_Chat_Message_MessageId",
                table: "Chat_HistoryMessage",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Session_Chat_Message_LastMessageId",
                table: "Chat_Session");

            migrationBuilder.DropTable(
                name: "Chat_ArticleMessage");

            migrationBuilder.DropTable(
                name: "Chat_ChatObjectCategoryUnit");

            migrationBuilder.DropTable(
                name: "Chat_Connection");

            migrationBuilder.DropTable(
                name: "Chat_FavoriteMessage");

            migrationBuilder.DropTable(
                name: "Chat_FriendshipTagUnit");

            migrationBuilder.DropTable(
                name: "Chat_HistoryMessage");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_ArticleContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_CmdContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_ContactsContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_FileContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_HistoryContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_HtmlContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_ImageContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_LinkContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_LocationContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_RedEnvelopeContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_SoundContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_TextContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_MapTo_VideoContent");

            migrationBuilder.DropTable(
                name: "Chat_MessageReminder");

            migrationBuilder.DropTable(
                name: "Chat_OpenedRecorder");

            migrationBuilder.DropTable(
                name: "Chat_ReadedRecorder");

            migrationBuilder.DropTable(
                name: "Chat_RedEnvelopeUnit");

            migrationBuilder.DropTable(
                name: "Chat_SessionSetting");

            migrationBuilder.DropTable(
                name: "Chat_SessionUnitRole");

            migrationBuilder.DropTable(
                name: "Chat_SessionUnitTag");

            migrationBuilder.DropTable(
                name: "Chat_WalletRequest");

            migrationBuilder.DropTable(
                name: "Chat_Article");

            migrationBuilder.DropTable(
                name: "Chat_ChatObjectCategory");

            migrationBuilder.DropTable(
                name: "Chat_Favorite");

            migrationBuilder.DropTable(
                name: "Chat_FriendshipTag");

            migrationBuilder.DropTable(
                name: "Chat_Friendship");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_ArticleContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_CmdContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_ContactsContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_FileContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_HistoryContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_HtmlContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_LinkContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_LocationContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_TextContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropTable(
                name: "Chat_Message_Template_RedEnvelopeContent");

            migrationBuilder.DropTable(
                name: "Chat_SessionRole");

            migrationBuilder.DropTable(
                name: "Chat_SessionTag");

            migrationBuilder.DropTable(
                name: "Chat_SessionUnit");

            migrationBuilder.DropTable(
                name: "Chat_PaymentPlatform");

            migrationBuilder.DropTable(
                name: "Chat_WalletRecorder");

            migrationBuilder.DropTable(
                name: "Chat_FriendshipRequest");

            migrationBuilder.DropTable(
                name: "Chat_WalletBusiness");

            migrationBuilder.DropTable(
                name: "Chat_Wallet");

            migrationBuilder.DropTable(
                name: "Chat_Message");

            migrationBuilder.DropTable(
                name: "Chat_MessageContent");

            migrationBuilder.DropTable(
                name: "Chat_Session");

            migrationBuilder.DropTable(
                name: "Chat_ChatObject");

            migrationBuilder.DropTable(
                name: "Chat_ChatObjectType");
        }
    }
}
