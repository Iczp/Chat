using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_RenameProp_LastMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "ReadedMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<long>(
                name: "LastMessageId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReadedMessageId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageId",
                table: "Chat_SessionUnit",
                column: "LastMessageId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId" },
                descending: new bool[0]);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Message_LastMessageId",
                table: "Chat_SessionUnit",
                column: "LastMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Message_ReadedMessageId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Message_LastMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Message_ReadedMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "ReadedMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<long>(
                name: "LastMessageAutoId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReadedMessageAutoId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit",
                column: "LastMessageAutoId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageAutoId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageAutoId",
                descending: new bool[0]);
        }
    }
}
