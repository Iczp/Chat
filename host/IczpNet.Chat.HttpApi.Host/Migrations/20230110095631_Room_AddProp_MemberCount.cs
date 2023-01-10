using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Room_AddProp_MemberCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "LastMessageAutoId",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<int>(
                name: "MemberCount",
                table: "Chat_Room",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberCount",
                table: "Chat_Room");

            migrationBuilder.AddColumn<long>(
                name: "LastMessageAutoId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_LastMessageAutoId",
                table: "Chat_SessionUnit",
                column: "LastMessageAutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageAutoId",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageAutoId" });
        }
    }
}
