using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_Id_Asc_CreationTime_Asc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_CreationTime",
                table: "Chat_Message",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_CreationTime_Asc",
                table: "Chat_Message",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Id_Asc",
                table: "Chat_Message",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_CreationTime",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_CreationTime_Asc",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Id_Asc",
                table: "Chat_Message");
        }
    }
}
