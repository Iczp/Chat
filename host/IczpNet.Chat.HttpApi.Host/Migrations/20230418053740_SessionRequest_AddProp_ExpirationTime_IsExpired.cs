using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionRequest_AddProp_ExpirationTime_IsExpired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationTime",
                table: "Chat_SessionRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpired",
                table: "Chat_SessionRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_Code",
                table: "Chat_ChatObject",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_FullPath",
                table: "Chat_ChatObject",
                column: "FullPath");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ChatObject_Name",
                table: "Chat_ChatObject",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_Code",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_FullPath",
                table: "Chat_ChatObject");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ChatObject_Name",
                table: "Chat_ChatObject");

            migrationBuilder.DropColumn(
                name: "ExpirationTime",
                table: "Chat_SessionRequest");

            migrationBuilder.DropColumn(
                name: "IsExpired",
                table: "Chat_SessionRequest");
        }
    }
}
