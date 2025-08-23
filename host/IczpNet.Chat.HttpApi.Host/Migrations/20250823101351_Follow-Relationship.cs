using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class FollowRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_DestinationId",
                table: "Chat_Follow");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_OwnerId",
                table: "Chat_Follow");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerSessionUnitId",
                table: "Chat_Follow");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Follow_DestinationSessionUnitId",
                table: "Chat_Follow",
                column: "DestinationSessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_DestinationId",
                table: "Chat_Follow",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_ChatObject_OwnerId",
                table: "Chat_Follow",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_DestinationSessionUnitId",
                table: "Chat_Follow",
                column: "DestinationSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerSessionUnitId",
                table: "Chat_Follow",
                column: "OwnerSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_DestinationSessionUnitId",
                table: "Chat_Follow");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerSessionUnitId",
                table: "Chat_Follow");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Follow_DestinationSessionUnitId",
                table: "Chat_Follow");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerSessionUnitId",
                table: "Chat_Follow",
                column: "OwnerSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
