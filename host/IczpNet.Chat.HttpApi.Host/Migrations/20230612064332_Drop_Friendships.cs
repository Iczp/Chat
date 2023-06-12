using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Drop_Friendships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_FriendshipTagUnit");

            migrationBuilder.DropTable(
                name: "Chat_FriendshipTag");

            migrationBuilder.DropTable(
                name: "Chat_Friendship");

            migrationBuilder.DropTable(
                name: "Chat_FriendshipRequest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_FriendshipRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HandlMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HandlTime = table.Column<DateTime>(type: "datetime2", maxLength: 200, nullable: true),
                    IsAgreed = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsHandled = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    table.PrimaryKey("PK_Chat_FriendshipTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_FriendshipTag_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Friendship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BackgroundImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCantacts = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false),
                    IsPassive = table.Column<bool>(type: "bit", nullable: false),
                    IsShowMemberName = table.Column<bool>(type: "bit", nullable: false),
                    IsShowRead = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortingNumber = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "Chat_FriendshipTagUnit",
                columns: table => new
                {
                    FriendshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FriendshipTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
        }
    }
}
