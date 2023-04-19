using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Motto_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MottoId",
                table: "Chat_ChatObject",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_Motto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Chat_Motto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Motto_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_MottoId",
                table: "Chat_ChatObject",
                column: "MottoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Motto_OwnerId",
                table: "Chat_Motto",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObject_Chat_Motto_MottoId",
                table: "Chat_ChatObject",
                column: "MottoId",
                principalTable: "Chat_Motto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObject_Chat_Motto_MottoId",
                table: "Chat_ChatObject");

            migrationBuilder.DropTable(
                name: "Chat_Motto");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_MottoId",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "MottoId",
                table: "Chat_ChatObject");
        }
    }
}
