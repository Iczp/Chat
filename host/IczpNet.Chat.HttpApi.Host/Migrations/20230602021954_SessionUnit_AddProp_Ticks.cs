using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_Ticks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Ticks",
                table: "Chat_SessionUnit",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Ticks",
                table: "Chat_SessionUnit",
                column: "Ticks",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Ticks",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "Ticks",
                table: "Chat_SessionUnit");
        }
    }
}
