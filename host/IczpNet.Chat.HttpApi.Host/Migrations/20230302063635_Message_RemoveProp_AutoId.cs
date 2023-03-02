using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_RemoveProp_AutoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_AutoId",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "AutoId",
                table: "Chat_Message");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Id",
                table: "Chat_Message",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Id",
                table: "Chat_Message");

            migrationBuilder.AddColumn<long>(
                name: "AutoId",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_AutoId",
                table: "Chat_Message",
                column: "AutoId");
        }
    }
}
