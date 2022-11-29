using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Room_Sections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImage",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultRoleId",
                table: "Chat_Room",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_Room",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationMethod",
                table: "Chat_Room",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowAutoJoin",
                table: "Chat_Room",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanSetBackground",
                table: "Chat_Room",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanSetImmersed",
                table: "Chat_Room",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForbiddenInput",
                table: "Chat_Room",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManagerUserIdList",
                table: "Chat_Room",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberNameDisplayMode",
                table: "Chat_Room",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerChatObjectId",
                table: "Chat_Room",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Portrait",
                table: "Chat_Room",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PortraitDisplayMode",
                table: "Chat_Room",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Chat_Room",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Chat_ForbiddenMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerChatObjectId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    OperatorChatObjectId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ForbiddenMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ForbiddenMember_Chat_Room_RoomId",
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
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomRole = table.Column<int>(type: "int", nullable: false),
                    MemberName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    HistoryFirstTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JoinWay = table.Column<int>(type: "int", nullable: false),
                    InviterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InputForbiddenExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PositionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    DepartmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
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
                    table.PrimaryKey("PK_Chat_RoomMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_RoomMember_Chat_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Chat_Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_RoomPermissionDefine",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultValue = table.Column<long>(type: "bigint", nullable: false),
                    MaxValue = table.Column<long>(type: "bigint", nullable: false),
                    MinValue = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Sorting = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "Chat_RoomRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Sorting = table.Column<long>(type: "bigint", nullable: false),
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
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefineId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
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
                    RoomMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_Chat_RoomRoleRoomMember", x => new { x.RoleId, x.RoomMemberId });
                    table.ForeignKey(
                        name: "FK_Chat_RoomRoleRoomMember_Chat_RoomMember_RoomMemberId",
                        column: x => x.RoomMemberId,
                        principalTable: "Chat_RoomMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_RoomRoleRoomMember_Chat_RoomRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Chat_RoomRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_DefaultRoleId",
                table: "Chat_Room",
                column: "DefaultRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Room_OwnerChatObjectId",
                table: "Chat_Room",
                column: "OwnerChatObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ForbiddenMember_RoomId",
                table: "Chat_ForbiddenMember",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_RoomMember_RoomId",
                table: "Chat_RoomMember",
                column: "RoomId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_OwnerChatObjectId",
                table: "Chat_Room",
                column: "OwnerChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Room_Chat_RoomRole_DefaultRoleId",
                table: "Chat_Room",
                column: "DefaultRoleId",
                principalTable: "Chat_RoomRole",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_ChatObject_OwnerChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Room_Chat_RoomRole_DefaultRoleId",
                table: "Chat_Room");

            migrationBuilder.DropTable(
                name: "Chat_ForbiddenMember");

            migrationBuilder.DropTable(
                name: "Chat_RoomPermissionGrant");

            migrationBuilder.DropTable(
                name: "Chat_RoomRoleRoomMember");

            migrationBuilder.DropTable(
                name: "Chat_RoomPermissionDefine");

            migrationBuilder.DropTable(
                name: "Chat_RoomMember");

            migrationBuilder.DropTable(
                name: "Chat_RoomRole");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_DefaultRoleId",
                table: "Chat_Room");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Room_OwnerChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "BackgroundImage",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "DefaultRoleId",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "InvitationMethod",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "IsAllowAutoJoin",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "IsCanSetBackground",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "IsCanSetImmersed",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "IsForbiddenInput",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "ManagerUserIdList",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "MemberNameDisplayMode",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "OwnerChatObjectId",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "Portrait",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "PortraitDisplayMode",
                table: "Chat_Room");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Chat_Room");
        }
    }
}
