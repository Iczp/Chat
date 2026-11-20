using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddCallCenterAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_CallCenterAssignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopKeeperId = table.Column<long>(type: "bigint", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    AgentId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallCenterAssignment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_CallCenterConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopKeeperId = table.Column<long>(type: "bigint", nullable: false),
                    DistributionStrategy = table.Column<int>(type: "int", nullable: false),
                    OverflowStrategy = table.Column<int>(type: "int", nullable: false),
                    MaxActiveConversations = table.Column<int>(type: "int", nullable: false),
                    ActiveTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    RequireOnline = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallCenterConfiguration", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallCenterAssignment_SessionId",
                table: "Chat_CallCenterAssignment",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallCenterAssignment_ShopKeeperId_AgentId_Status_LastActivityAt",
                table: "Chat_CallCenterAssignment",
                columns: new[] { "ShopKeeperId", "AgentId", "Status", "LastActivityAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallCenterAssignment_ShopKeeperId_Status_QueuedAt_Id",
                table: "Chat_CallCenterAssignment",
                columns: new[] { "ShopKeeperId", "Status", "QueuedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallCenterConfiguration_ShopKeeperId",
                table: "Chat_CallCenterConfiguration",
                column: "ShopKeeperId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_CallCenterAssignment");

            migrationBuilder.DropTable(
                name: "Chat_CallCenterConfiguration");
        }
    }
}
