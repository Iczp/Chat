using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitCounter_AddProp_LastMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastMessageId",
                table: "Chat_SessionUnitCounter",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitCounter_LastMessageId",
                table: "Chat_SessionUnitCounter",
                column: "LastMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnitCounter_Chat_Message_LastMessageId",
                table: "Chat_SessionUnitCounter",
                column: "LastMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnitCounter_Chat_Message_LastMessageId",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnitCounter_LastMessageId",
                table: "Chat_SessionUnitCounter");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                table: "Chat_SessionUnitCounter");
        }
    }
}
