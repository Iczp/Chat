using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class FllowOwnerSessionUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Follow");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_Follow",
                newName: "OwnerSessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerSessionUnitId",
                table: "Chat_Follow",
                column: "OwnerSessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerSessionUnitId",
                table: "Chat_Follow");

            migrationBuilder.RenameColumn(
                name: "OwnerSessionUnitId",
                table: "Chat_Follow",
                newName: "SessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Follow",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
