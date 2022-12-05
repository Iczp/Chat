using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionSetting_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImage",
                table: "Chat_SessionSetting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCantacts",
                table: "Chat_SessionSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImmersed",
                table: "Chat_SessionSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShowMemberName",
                table: "Chat_SessionSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShowRead",
                table: "Chat_SessionSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "Chat_SessionSetting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rename",
                table: "Chat_SessionSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SortingNumber",
                table: "Chat_SessionSetting",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImage",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "IsCantacts",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "IsImmersed",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "IsShowMemberName",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "IsShowRead",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "Rename",
                table: "Chat_SessionSetting");

            migrationBuilder.DropColumn(
                name: "SortingNumber",
                table: "Chat_SessionSetting");
        }
    }
}
