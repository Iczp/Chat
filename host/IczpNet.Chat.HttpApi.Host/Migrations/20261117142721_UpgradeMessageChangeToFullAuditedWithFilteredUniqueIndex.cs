using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeMessageChangeToFullAuditedWithFilteredUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Chat_MessageChange",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_MessageChange",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_MessageChange",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_MessageChange",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Chat_MessageChange",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Chat_MessageChange",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE c
SET c.IsDeleted = 1,
    c.DeletionTime = SYSUTCDATETIME()
FROM Chat_MessageChange c
WHERE c.Id < (
    SELECT MAX(c2.Id)
    FROM Chat_MessageChange c2
    WHERE c2.MessageId = c.MessageId
);");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageChange_IsDeleted_DeletionTime",
                table: "Chat_MessageChange",
                columns: new[] { "IsDeleted", "DeletionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageChange_MessageId",
                table: "Chat_MessageChange",
                column: "MessageId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageChange_IsDeleted_DeletionTime",
                table: "Chat_MessageChange");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageChange_MessageId",
                table: "Chat_MessageChange");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chat_MessageChange");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_MessageChange");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_MessageChange");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_MessageChange");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Chat_MessageChange");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Chat_MessageChange");
        }
    }
}
