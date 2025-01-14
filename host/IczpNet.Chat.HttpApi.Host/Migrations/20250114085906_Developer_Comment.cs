using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Developer_Comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "Chat_Developer",
                comment: "开发者");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "Chat_Developer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "Token",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostUrl",
                table: "Chat_Developer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                comment: "开发者设置的Url",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Developer",
                type: "bit",
                nullable: false,
                comment: "是否启用开发者",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "EncodingAesKey",
                table: "Chat_Developer",
                type: "nvarchar(43)",
                maxLength: 43,
                nullable: true,
                comment: "EncodingAesKey",
                oldClrType: typeof(string),
                oldType: "nvarchar(43)",
                oldMaxLength: 43,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "Chat_Developer",
                oldComment: "开发者");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "Chat_Developer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Token");

            migrationBuilder.AlterColumn<string>(
                name: "PostUrl",
                table: "Chat_Developer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldComment: "开发者设置的Url");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "Chat_Developer",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "是否启用开发者");

            migrationBuilder.AlterColumn<string>(
                name: "EncodingAesKey",
                table: "Chat_Developer",
                type: "nvarchar(43)",
                maxLength: 43,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(43)",
                oldMaxLength: 43,
                oldNullable: true,
                oldComment: "EncodingAesKey");
        }
    }
}
