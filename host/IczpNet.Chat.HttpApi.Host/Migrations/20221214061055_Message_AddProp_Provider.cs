using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_AddProp_Provider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Chat_Message",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ArticleMessage_Chat_Message_MessageId",
                table: "Chat_ArticleMessage",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ArticleMessage_Chat_Message_MessageId",
                table: "Chat_ArticleMessage");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Chat_Message");
        }
    }
}
