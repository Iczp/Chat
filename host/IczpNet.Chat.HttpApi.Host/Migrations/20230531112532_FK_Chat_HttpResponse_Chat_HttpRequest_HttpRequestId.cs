using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse",
                column: "HttpRequestId",
                principalTable: "Chat_HttpRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse",
                column: "HttpRequestId",
                principalTable: "Chat_HttpRequest",
                principalColumn: "Id");
        }
    }
}
