using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Follow_AlertProp_SessionUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerId",
                table: "Chat_Follow");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Follow");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_Follow",
                newName: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Follow_Chat_SessionUnit_OwnerId",
                table: "Chat_Follow",
                column: "OwnerId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
