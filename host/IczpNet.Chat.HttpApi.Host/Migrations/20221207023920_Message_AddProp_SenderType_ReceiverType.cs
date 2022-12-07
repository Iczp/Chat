using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_AddProp_SenderType_ReceiverType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiverType",
                table: "Chat_Message",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderType",
                table: "Chat_Message",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ReceiverType",
                table: "Chat_Message",
                column: "ReceiverType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_SenderType",
                table: "Chat_Message",
                column: "SenderType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ReceiverType",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_SenderType",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ReceiverType",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "SenderType",
                table: "Chat_Message");
        }
    }
}
