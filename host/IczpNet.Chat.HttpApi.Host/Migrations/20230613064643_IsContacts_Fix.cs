using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class IsContacts_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCantacts",
                table: "Chat_SessionUnitSetting",
                newName: "IsContacts");

            migrationBuilder.RenameColumn(
                name: "IsCantacts",
                table: "Chat_SessionSetting",
                newName: "IsContacts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsContacts",
                table: "Chat_SessionUnitSetting",
                newName: "IsCantacts");

            migrationBuilder.RenameColumn(
                name: "IsContacts",
                table: "Chat_SessionSetting",
                newName: "IsCantacts");
        }
    }
}
