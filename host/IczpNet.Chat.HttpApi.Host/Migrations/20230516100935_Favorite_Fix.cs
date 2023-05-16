using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Favorite_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Favorite_Chat_ChatObject_OwnerId",
                table: "Chat_Favorite");

            migrationBuilder.DropTable(
                name: "Chat_FavoriteMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Favorite",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_Favorite");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Chat_Favorite",
                newName: "SessionUnitId");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_Favorite",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "MessageId",
                table: "Chat_Favorite",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DestinationId",
                table: "Chat_Favorite",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Chat_Favorite",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Favorite",
                table: "Chat_Favorite",
                columns: new[] { "SessionUnitId", "MessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Favorite_DestinationId",
                table: "Chat_Favorite",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Favorite_MessageId",
                table: "Chat_Favorite",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Favorite_Chat_ChatObject_DestinationId",
                table: "Chat_Favorite",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Favorite_Chat_ChatObject_OwnerId",
                table: "Chat_Favorite",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Favorite_Chat_Message_MessageId",
                table: "Chat_Favorite",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Favorite_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Favorite",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Favorite_Chat_ChatObject_DestinationId",
                table: "Chat_Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Favorite_Chat_ChatObject_OwnerId",
                table: "Chat_Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Favorite_Chat_Message_MessageId",
                table: "Chat_Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Favorite_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Favorite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_Favorite",
                table: "Chat_Favorite");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Favorite_DestinationId",
                table: "Chat_Favorite");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Favorite_MessageId",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_Favorite");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Chat_Favorite");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_Favorite",
                newName: "Id");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_Favorite",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_Favorite",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_Favorite",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_Favorite",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_Favorite",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_Favorite",
                table: "Chat_Favorite",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Chat_FavoriteMessage",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    FavoriteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_FavoriteMessage", x => new { x.MessageId, x.FavoriteId });
                    table.ForeignKey(
                        name: "FK_Chat_FavoriteMessage_Chat_Favorite_FavoriteId",
                        column: x => x.FavoriteId,
                        principalTable: "Chat_Favorite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_FavoriteMessage_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_FavoriteMessage_FavoriteId",
                table: "Chat_FavoriteMessage",
                column: "FavoriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Favorite_Chat_ChatObject_OwnerId",
                table: "Chat_Favorite",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
