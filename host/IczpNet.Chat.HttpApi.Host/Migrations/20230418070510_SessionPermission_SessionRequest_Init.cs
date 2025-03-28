﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionPermission_SessionRequest_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_SessionRole",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Chat_SessionRole",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Chat_SessionPermissionGroup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FullPathName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    Sorting = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionPermissionGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionPermissionGroup_Chat_SessionPermissionGroup_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Chat_SessionPermissionGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    RequestMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsHandled = table.Column<bool>(type: "bit", nullable: false),
                    IsAgreed = table.Column<bool>(type: "bit", nullable: true),
                    HandleMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HandleTime = table.Column<DateTime>(type: "datetime2", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HandlerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Chat_SessionRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionRequest_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionRequest_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionRequest_Chat_SessionUnit_HandlerId",
                        column: x => x.HandlerId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionPermissionDefinition",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultValue = table.Column<long>(type: "bigint", nullable: false),
                    MaxValue = table.Column<long>(type: "bigint", nullable: false),
                    MinValue = table.Column<long>(type: "bigint", nullable: false),
                    Sorting = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Chat_SessionPermissionDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionPermissionDefinition_Chat_SessionPermissionGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Chat_SessionPermissionGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionPermissionRoleGrant",
                columns: table => new
                {
                    DefinitionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionPermissionRoleGrant", x => new { x.DefinitionId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionPermissionRoleGrant_Chat_SessionPermissionDefinition_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "Chat_SessionPermissionDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionPermissionRoleGrant_Chat_SessionRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Chat_SessionRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chat_SessionPermissionUnitGrant",
                columns: table => new
                {
                    DefinitionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionPermissionUnitGrant", x => new { x.DefinitionId, x.SessionUnitId });
                    table.ForeignKey(
                        name: "FK_Chat_SessionPermissionUnitGrant_Chat_SessionPermissionDefinition_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "Chat_SessionPermissionDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionPermissionUnitGrant_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRole_IsDefault",
                table: "Chat_SessionRole",
                column: "IsDefault",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_Code",
                table: "Chat_ChatObject",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_FullPath",
                table: "Chat_ChatObject",
                column: "FullPath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_Name",
                table: "Chat_ChatObject",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionDefinition_GroupId",
                table: "Chat_SessionPermissionDefinition",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionDefinition_Sorting",
                table: "Chat_SessionPermissionDefinition",
                column: "Sorting",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionGroup_FullPath",
                table: "Chat_SessionPermissionGroup",
                column: "FullPath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionGroup_ParentId",
                table: "Chat_SessionPermissionGroup",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionGroup_Sorting",
                table: "Chat_SessionPermissionGroup",
                column: "Sorting",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionRoleGrant_RoleId",
                table: "Chat_SessionPermissionRoleGrant",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionPermissionUnitGrant_SessionUnitId",
                table: "Chat_SessionPermissionUnitGrant",
                column: "SessionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_DestinationId",
                table: "Chat_SessionRequest",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_HandlerId",
                table: "Chat_SessionRequest",
                column: "HandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_OwnerId",
                table: "Chat_SessionRequest",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionPermissionRoleGrant");

            migrationBuilder.DropTable(
                name: "Chat_SessionPermissionUnitGrant");

            migrationBuilder.DropTable(
                name: "Chat_SessionRequest");

            migrationBuilder.DropTable(
                name: "Chat_SessionPermissionDefinition");

            migrationBuilder.DropTable(
                name: "Chat_SessionPermissionGroup");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRole_IsDefault",
                table: "Chat_SessionRole");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_Code",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_FullPath",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_Name",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_SessionRole");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Chat_SessionRole");
        }
    }
}
