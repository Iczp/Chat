using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageFollower_Add_FollowerCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderFollerIds",
                table: "Chat_Message",
                newName: "SenderFollowerIds");

            migrationBuilder.AddColumn<int>(
                name: "FollowerCount",
                table: "Chat_Message",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "关注数量");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowerCount",
                table: "Chat_Message");

            migrationBuilder.RenameColumn(
                name: "SenderFollowerIds",
                table: "Chat_Message",
                newName: "SenderFollerIds");
        }
    }
}
