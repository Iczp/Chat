using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_RenameSpelling_RenameSpellingAbbreviation_MemberNameSpelling_MemberNameSpellingAbbreviation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnit",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenameSpelling",
                table: "Chat_SessionUnit",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "RenameSpelling",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnit");
        }
    }
}
