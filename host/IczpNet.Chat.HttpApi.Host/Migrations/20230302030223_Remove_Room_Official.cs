using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Room_Official : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_RoomRole_DefaultRoleId",
                table: "Chat_Room");

            migrationBuilder.DropTable(
                name: "Chat_ChatUser");

            migrationBuilder.DropTable(
                name: "Chat_OfficalExcludedMember");

            migrationBuilder.DropTable(
                name: "Chat_OfficialGroupMember");

            migrationBuilder.DropTable(
                name: "Chat_OfficialMemberTagUnit");

            migrationBuilder.DropTable(
                name: "Chat_Robot");

            migrationBuilder.DropTable(
                name: "Chat_RoomForbiddenMember");

            migrationBuilder.DropTable(
                name: "Chat_RoomPermissionGrant");

            migrationBuilder.DropTable(
                name: "Chat_RoomRoleRoomMember");

            migrationBuilder.DropTable(
                name: "Chat_ShopWaiter");

            migrationBuilder.DropTable(
                name: "Chat_SquareMember");

            migrationBuilder.DropTable(
                name: "Chat_Subscription");

            migrationBuilder.DropTable(
                name: "Chat_OfficialGroup");

            migrationBuilder.DropTable(
                name: "Chat_OfficialMemberTag");

            migrationBuilder.DropTable(
                name: "Chat_OfficialMember");

            migrationBuilder.DropTable(
                name: "Chat_RoomPermissionDefine");

            migrationBuilder.DropTable(
                name: "Chat_RoomMember");

            migrationBuilder.DropTable(
                name: "Chat_ShopKeeper");

            migrationBuilder.DropTable(
                name: "Chat_Square");

            migrationBuilder.DropTable(
                name: "Chat_Official");

            migrationBuilder.DropTable(
                name: "Chat_SquareCategory");

            migrationBuilder.DropTable(
                name: "Chat_RoomRole");

            migrationBuilder.DropTable(
                name: "Chat_Room");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ChatUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ChatUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ChatUser_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Official",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Official", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Official_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Official_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Robot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RobotType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Robot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Robot_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomPermissionDefine",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultValue = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxValue = table.Column<long>(type: "bigint", nullable: false),
                    MinValue = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sorting = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RoomPermissionDefine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_RoomPermissionDefine_Chat_RoomPermissionDefine_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Chat_RoomPermissionDefine",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_ShopKeeper",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ShopKeeper", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ShopKeeper_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ShopKeeper_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SquareCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FullPathName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Sorting = table.Column<double>(type: "float", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SquareCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SquareCategory_Chat_SquareCategory_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Chat_SquareCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Subscription_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Subscription_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_OfficalExcludedMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OfficalExcludedMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_OfficalExcludedMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_OfficalExcludedMember_Chat_Official_OfficialId",
                        column: x => x.OfficialId,
                        principalTable: "Chat_Official",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_OfficialGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OfficialGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                        column: x => x.OfficialId,
                        principalTable: "Chat_Official",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_OfficialMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OfficialMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialMember_Chat_Official_OfficialId",
                        column: x => x.OfficialId,
                        principalTable: "Chat_Official",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_OfficialMemberTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OfficialMemberTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialMemberTag_Chat_Official_OfficialId",
                        column: x => x.OfficialId,
                        principalTable: "Chat_Official",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_ShopWaiter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShopKeeperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ShopWaiter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ShopWaiter_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ShopWaiter_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_ShopWaiter_Chat_ShopKeeper_ShopKeeperId",
                        column: x => x.ShopKeeperId,
                        principalTable: "Chat_ShopKeeper",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Square",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Square", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Square_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Square_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Square_Chat_SquareCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Chat_SquareCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_OfficialGroupMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficialGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OfficialGroupMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialGroupMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialGroupMember_Chat_OfficialGroup_OfficialGroupId",
                        column: x => x.OfficialGroupId,
                        principalTable: "Chat_OfficialGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_OfficialMemberTagUnit",
                columns: table => new
                {
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_OfficialMemberTagUnit", x => new { x.TagId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_Chat_OfficialMemberTagUnit_Chat_OfficialMemberTag_TagId",
                        column: x => x.TagId,
                        principalTable: "Chat_OfficialMemberTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_OfficialMemberTagUnit_Chat_OfficialMember_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Chat_OfficialMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SquareMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SquareId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoryFirstTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MemberName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SquareMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SquareMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SquareMember_Chat_Square_SquareId",
                        column: x => x.SquareId,
                        principalTable: "Chat_Square",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Room",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BackgroundImage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InvitationMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsAllowAutoJoin = table.Column<bool>(type: "bit", nullable: false),
                    IsCanSetBackground = table.Column<bool>(type: "bit", nullable: false),
                    IsCanSetImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsForbiddenAll = table.Column<bool>(type: "bit", nullable: false),
                    MemberCount = table.Column<int>(type: "int", nullable: false),
                    MemberNameDisplayMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PortraitDisplayMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Room", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Room_Chat_ChatObject_Id",
                        column: x => x.Id,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Room_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Room_Chat_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Chat_Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomForbiddenMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperatorChatObjectId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RoomForbiddenMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_RoomForbiddenMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_RoomForbiddenMember_Chat_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Chat_Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InviterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoryFirstTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InputForbiddenExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    JoinWay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MemberName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RoomRoleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RoomMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_RoomMember_Chat_ChatObject_InviterId",
                        column: x => x.InviterId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_RoomMember_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_RoomMember_Chat_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Chat_Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Sorting = table.Column<double>(type: "float", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RoomRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_RoomRole_Chat_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Chat_Room",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomPermissionGrant",
                columns: table => new
                {
                    DefineId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RoomPermissionGrant", x => new { x.DefineId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Chat_RoomPermissionGrant_Chat_RoomPermissionDefine_DefineId",
                        column: x => x.DefineId,
                        principalTable: "Chat_RoomPermissionDefine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_RoomPermissionGrant_Chat_RoomRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Chat_RoomRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomRoleRoomMember",
                columns: table => new
                {
                    RoomRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_RoomRoleRoomMember", x => new { x.RoomRoleId, x.RoomMemberId });
                    table.ForeignKey(
                        name: "FK_Chat_RoomRoleRoomMember_Chat_RoomMember_RoomMemberId",
                        column: x => x.RoomMemberId,
                        principalTable: "Chat_RoomMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_RoomRoleRoomMember_Chat_RoomRole_RoomRoleId",
                        column: x => x.RoomRoleId,
                        principalTable: "Chat_RoomRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficalExcludedMember_OfficialId",
                table: "Chat_OfficalExcludedMember",
                column: "OfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficalExcludedMember_OwnerId",
                table: "Chat_OfficalExcludedMember",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Official_SessionId",
                table: "Chat_Official",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Official_Type",
                table: "Chat_Official",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialGroup_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialGroupMember_OfficialGroupId",
                table: "Chat_OfficialGroupMember",
                column: "OfficialGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialGroupMember_OwnerId",
                table: "Chat_OfficialGroupMember",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialMember_OfficialId",
                table: "Chat_OfficialMember",
                column: "OfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialMember_OwnerId",
                table: "Chat_OfficialMember",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialMemberTag_OfficialId",
                table: "Chat_OfficialMemberTag",
                column: "OfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialMemberTagUnit_MemberId",
                table: "Chat_OfficialMemberTagUnit",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Robot_RobotType",
                table: "Chat_Robot",
                column: "RobotType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_DefaultRoleId",
                table: "Chat_Room",
                column: "DefaultRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_InvitationMethod",
                table: "Chat_Room",
                column: "InvitationMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_MemberNameDisplayMode",
                table: "Chat_Room",
                column: "MemberNameDisplayMode");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_OwnerId",
                table: "Chat_Room",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_PortraitDisplayMode",
                table: "Chat_Room",
                column: "PortraitDisplayMode");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_SessionId",
                table: "Chat_Room",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_Type",
                table: "Chat_Room",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomForbiddenMember_OwnerId",
                table: "Chat_RoomForbiddenMember",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomForbiddenMember_RoomId",
                table: "Chat_RoomForbiddenMember",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_InviterId",
                table: "Chat_RoomMember",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_JoinWay",
                table: "Chat_RoomMember",
                column: "JoinWay");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_OwnerId",
                table: "Chat_RoomMember",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_RoomId",
                table: "Chat_RoomMember",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_RoomRoleType",
                table: "Chat_RoomMember",
                column: "RoomRoleType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomPermissionDefine_ParentId",
                table: "Chat_RoomPermissionDefine",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomPermissionGrant_RoleId",
                table: "Chat_RoomPermissionGrant",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomRole_RoomId",
                table: "Chat_RoomRole",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomRoleRoomMember_RoomMemberId",
                table: "Chat_RoomRoleRoomMember",
                column: "RoomMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopKeeper_OwnerId",
                table: "Chat_ShopKeeper",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopWaiter_OwnerId",
                table: "Chat_ShopWaiter",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ShopWaiter_ShopKeeperId",
                table: "Chat_ShopWaiter",
                column: "ShopKeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Square_CategoryId",
                table: "Chat_Square",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Square_SessionId",
                table: "Chat_Square",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Square_Type",
                table: "Chat_Square",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SquareCategory_ParentId",
                table: "Chat_SquareCategory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SquareMember_OwnerId",
                table: "Chat_SquareMember",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SquareMember_SquareId",
                table: "Chat_SquareMember",
                column: "SquareId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Subscription_SessionId",
                table: "Chat_Subscription",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_RoomRole_DefaultRoleId",
                table: "Chat_Room",
                column: "DefaultRoleId",
                principalTable: "Chat_RoomRole",
                principalColumn: "Id");
        }
    }
}
