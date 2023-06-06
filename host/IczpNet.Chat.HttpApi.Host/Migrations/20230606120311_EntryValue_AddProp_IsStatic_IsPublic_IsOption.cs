using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class EntryValue_AddProp_IsStatic_IsPublic_IsOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOption",
                table: "Chat_EntryValue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Chat_EntryValue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStatic",
                table: "Chat_EntryValue",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOption",
                table: "Chat_EntryValue");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Chat_EntryValue");

            migrationBuilder.DropColumn(
                name: "IsStatic",
                table: "Chat_EntryValue");
        }
    }
}
