using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_Remove_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImage",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsCreator",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsInputEnabled",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsScoped",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "JoinWay",
                table: "Chat_SessionUnit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImage",
                table: "Chat_SessionUnit",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreator",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInputEnabled",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScoped",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JoinWay",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: true);
        }
    }
}
