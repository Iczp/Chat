using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddProp_DestinationObjectType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationObjectType",
                table: "Chat_SessionUnit",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_DestinationObjectType",
                table: "Chat_SessionUnit",
                column: "DestinationObjectType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_DestinationObjectType",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "DestinationObjectType",
                table: "Chat_SessionUnit");
        }
    }
}
