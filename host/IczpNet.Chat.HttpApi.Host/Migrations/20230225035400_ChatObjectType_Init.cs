using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ChatObjectType_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChatObjectTypeId",
                table: "Chat_ChatObject",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_ChatObjectType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxDepth = table.Column<int>(type: "int", nullable: false),
                    IsHasChild = table.Column<bool>(type: "bit", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_ChatObjectTypeId",
                table: "Chat_ChatObject",
                column: "ChatObjectTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObjectType_ChatObjectTypeId",
                table: "Chat_ChatObject",
                column: "ChatObjectTypeId",
                principalTable: "Chat_ChatObjectType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObject_Chat_ChatObjectType_ChatObjectTypeId",
                table: "Chat_ChatObject");

            migrationBuilder.DropTable(
                name: "Chat_ChatObjectType");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_ChatObjectTypeId",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "ChatObjectTypeId",
                table: "Chat_ChatObject");
        }
    }
}
