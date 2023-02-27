using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ChatObjectCategory_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Chat_ChatObject");

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

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectCategory_ChatObjectTypeId",
                table: "Chat_ChatObjectCategory",
                column: "ChatObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectCategory_ParentId",
                table: "Chat_ChatObjectCategory",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ChatObjectCategory");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Chat_ChatObject",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObject_GroupId",
                table: "Chat_ChatObject",
                column: "GroupId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
