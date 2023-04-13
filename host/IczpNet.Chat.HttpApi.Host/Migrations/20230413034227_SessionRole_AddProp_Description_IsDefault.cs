using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionRole_AddProp_Description_IsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_SessionRole",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Chat_SessionRole",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRole_IsDefault",
                table: "Chat_SessionRole",
                column: "IsDefault",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRole_IsDefault",
                table: "Chat_SessionRole");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_SessionRole");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Chat_SessionRole");
        }
    }
}
