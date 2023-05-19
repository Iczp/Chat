using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ServiceStatus_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceStatus",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<int>(
                name: "ServiceStatus",
                table: "Chat_ChatObject",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceStatus",
                table: "Chat_ChatObject");

            migrationBuilder.AddColumn<int>(
                name: "ServiceStatus",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
