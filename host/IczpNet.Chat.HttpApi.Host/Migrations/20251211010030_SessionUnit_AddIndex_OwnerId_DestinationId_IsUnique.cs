using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddIndex_OwnerId_DestinationId_IsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_DestinationId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_OwnerId",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId_DestinationId",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "DestinationId" },
                unique: true,
                filter: "[DestinationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId_SessionId",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "SessionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_DestinationId",
                table: "Chat_SessionUnit",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_DestinationId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_OwnerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_DestinationId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_SessionId",
                table: "Chat_SessionUnit");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_DestinationId",
                table: "Chat_SessionUnit",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
