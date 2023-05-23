using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class TreeEntity_AddProp_ChildrenCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "Chat_SessionPermissionGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "Chat_SessionOrganization",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "Chat_ChatObjectCategory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "Chat_ChatObject",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "Chat_SessionPermissionGroup");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "Chat_SessionOrganization");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "Chat_ChatObjectCategory");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "Chat_ChatObject");
        }
    }
}
