using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionRole_Add_PermissionCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PermissionCount",
                table: "Chat_SessionRole",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRole_PermissionCount",
                table: "Chat_SessionRole",
                column: "PermissionCount",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionRole_PermissionCount",
                table: "Chat_SessionRole");

            migrationBuilder.DropColumn(
                name: "PermissionCount",
                table: "Chat_SessionRole");
        }
    }
}
