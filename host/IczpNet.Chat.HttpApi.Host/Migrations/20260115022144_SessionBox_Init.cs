using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionBox_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoxId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true,
                comment: "会话盒子Id");

            migrationBuilder.CreateTable(
                name: "Chat_Box",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "名称"),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "说明"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Chat_Box", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Box_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                },
                comment: "会话盒子");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_BoxId",
                table: "Chat_SessionUnit",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Box_Name",
                table: "Chat_Box",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Box_OwnerId",
                table: "Chat_Box",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Box_BoxId",
                table: "Chat_SessionUnit",
                column: "BoxId",
                principalTable: "Chat_Box",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Box_BoxId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropTable(
                name: "Chat_Box");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_BoxId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "BoxId",
                table: "Chat_SessionUnit");
        }
    }
}
