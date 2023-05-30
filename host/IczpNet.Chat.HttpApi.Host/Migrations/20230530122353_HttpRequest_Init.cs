using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class HttpRequest_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_HttpRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cookies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Referer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Headers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ResponseContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<long>(type: "bigint", nullable: false),
                    EndTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_HttpRequest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_HttpMethod",
                table: "Chat_HttpRequest",
                column: "HttpMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_IsSuccess",
                table: "Chat_HttpRequest",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_StatusCode",
                table: "Chat_HttpRequest",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_Url",
                table: "Chat_HttpRequest",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_HttpRequest");
        }
    }
}
