using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Developer_HttpRequest_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiUrl",
                table: "Chat_Developer");

            migrationBuilder.AddColumn<string>(
                name: "EncodingAesKey",
                table: "Chat_Developer",
                type: "nvarchar(43)",
                maxLength: 43,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostUrl",
                table: "Chat_Developer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Chat_Developer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chat_HttpRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Scheme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Port = table.Column<int>(type: "int", nullable: false),
                    IsDefaultPort = table.Column<bool>(type: "bit", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fragment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AbsolutePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cookies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Referer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Headers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<long>(type: "bigint", nullable: false),
                    EndTime = table.Column<long>(type: "bigint", nullable: false),
                    ContentLength = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_HttpRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_HttpResponse",
                columns: table => new
                {
                    HttpRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_HttpResponse", x => x.HttpRequestId);
                    table.ForeignKey(
                        name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                        column: x => x.HttpRequestId,
                        principalTable: "Chat_HttpRequest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_AbsolutePath",
                table: "Chat_HttpRequest",
                column: "AbsolutePath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_Host",
                table: "Chat_HttpRequest",
                column: "Host");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_HttpMethod",
                table: "Chat_HttpRequest",
                column: "HttpMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_IsSuccess",
                table: "Chat_HttpRequest",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_Port",
                table: "Chat_HttpRequest",
                column: "Port");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_Scheme",
                table: "Chat_HttpRequest",
                column: "Scheme");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_HttpRequest_StatusCode",
                table: "Chat_HttpRequest",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_HttpResponse");

            migrationBuilder.DropTable(
                name: "Chat_HttpRequest");

            migrationBuilder.DropColumn(
                name: "EncodingAesKey",
                table: "Chat_Developer");

            migrationBuilder.DropColumn(
                name: "PostUrl",
                table: "Chat_Developer");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Chat_Developer");

            migrationBuilder.AddColumn<string>(
                name: "ApiUrl",
                table: "Chat_Developer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
