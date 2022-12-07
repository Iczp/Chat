using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class OfficialGroup_Inherit_ChatObject_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialGroup_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId",
                principalTable: "Chat_Official",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropIndex(
                name: "IX_Chat_OfficialGroup_OfficialId",
                table: "Chat_OfficialGroup");
        }
    }
}
