using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Developer_AddProp_Token_EncodingAesKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiUrl",
                table: "Chat_Developer");

            migrationBuilder.AddColumn<string>(
                name: "EncodingAesKey",
                table: "Chat_Developer",
                type: "nvarchar(43)",
                maxLength: 43,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostUrl",
                table: "Chat_Developer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Chat_Developer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncodingAesKey",
                table: "Chat_Developer");

            migrationBuilder.DropColumn(
                name: "PostUrl",
                table: "Chat_Developer");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Chat_Developer");

            migrationBuilder.AddColumn<string>(
                name: "ApiUrl",
                table: "Chat_Developer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
