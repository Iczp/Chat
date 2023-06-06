using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ChatObjectEntryValue_DropProp_DestinationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_DestinationId",
                table: "Chat_ChatObjectEntryValue");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObjectEntryValue_DestinationId",
                table: "Chat_ChatObjectEntryValue");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Chat_ChatObjectEntryValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DestinationId",
                table: "Chat_ChatObjectEntryValue",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObjectEntryValue_DestinationId",
                table: "Chat_ChatObjectEntryValue",
                column: "DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_DestinationId",
                table: "Chat_ChatObjectEntryValue",
                column: "DestinationId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");
        }
    }
}
