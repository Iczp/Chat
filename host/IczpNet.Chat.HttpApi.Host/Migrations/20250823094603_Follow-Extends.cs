using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class FollowExtends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DestinationId",
                table: "Chat_Follow",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationType",
                table: "Chat_Follow",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Chat_Follow",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "Chat_Follow",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Follow_DestinationId",
                table: "Chat_Follow",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Follow_OwnerId",
                table: "Chat_Follow",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_DestinationId",
                table: "Chat_Follow",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_OwnerId",
                table: "Chat_Follow",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_DestinationId",
                table: "Chat_Follow");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_OwnerId",
                table: "Chat_Follow");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Follow_DestinationId",
                table: "Chat_Follow");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Follow_OwnerId",
                table: "Chat_Follow");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_Follow");

            migrationBuilder.DropColumn(
                name: "DestinationType",
                table: "Chat_Follow");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat_Follow");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "Chat_Follow");
        }
    }
}
