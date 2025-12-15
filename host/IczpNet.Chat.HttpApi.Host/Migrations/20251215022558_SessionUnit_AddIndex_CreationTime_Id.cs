using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddIndex_CreationTime_Id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_SessionId",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId_CreationTime_Id",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "CreationTime", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_SessionId_CreationTime_Id",
                table: "Chat_SessionUnit",
                columns: new[] { "SessionId", "CreationTime", "Id" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_CreationTime_Id",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_SessionId_CreationTime_Id",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_SessionId",
                table: "Chat_SessionUnit",
                column: "SessionId");
        }
    }
}
