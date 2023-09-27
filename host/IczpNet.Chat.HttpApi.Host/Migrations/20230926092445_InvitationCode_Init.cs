using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class InvitationCode_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_InvitationCode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "邀请码"),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "过期时间"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true, comment: "所属聊天对象"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "是否可用"),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_Chat_InvitationCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_InvitationCode_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_InvitationCode_Code",
                table: "Chat_InvitationCode",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_InvitationCode_OwnerId",
                table: "Chat_InvitationCode",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_InvitationCode");
        }
    }
}
