using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageCounter_DropColumn_ReadedCount_OpenedCount_FavoritedCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavoritedCount",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "OpenedCount",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ReadedCount",
                table: "Chat_Message");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoritedCount",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpenedCount",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReadedCount",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
