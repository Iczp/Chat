using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class MessageContent_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MessageContentId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_MessageContentId",
                table: "Chat_Message",
                column: "MessageContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_MessageContent_MessageContentId",
                table: "Chat_Message",
                column: "MessageContentId",
                principalTable: "Chat_MessageContent",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_MessageContent_MessageContentId",
                table: "Chat_Message");

            migrationBuilder.DropTable(
                name: "Chat_MessageContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_MessageContentId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "MessageContentId",
                table: "Chat_Message");
        }
    }
}
