using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Entry_AddProp_XXX : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_EntryName",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Chat_EntryName",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Chat_EntryName",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUniqued",
                table: "Chat_EntryName",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxCount",
                table: "Chat_EntryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxLenth",
                table: "Chat_EntryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinCount",
                table: "Chat_EntryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinLenth",
                table: "Chat_EntryName",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Regex",
                table: "Chat_EntryName",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "IsUniqued",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "MaxCount",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "MaxLenth",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "MinCount",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "MinLenth",
                table: "Chat_EntryName");

            migrationBuilder.DropColumn(
                name: "Regex",
                table: "Chat_EntryName");
        }
    }
}
