using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class SessionUnit_AddProp_Id_K : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_SessionUnit",
                table: "Chat_SessionUnit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_SessionUnit",
                table: "Chat_SessionUnit",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_SessionId",
                table: "Chat_SessionUnit",
                column: "SessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_SessionUnit",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_SessionId",
                table: "Chat_SessionUnit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_SessionUnit",
                table: "Chat_SessionUnit",
                columns: new[] { "SessionId", "OwnerId" });
        }
    }
}
