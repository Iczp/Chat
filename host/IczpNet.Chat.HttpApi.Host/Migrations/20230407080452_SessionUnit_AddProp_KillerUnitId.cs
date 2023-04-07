using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_KillerUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KillerUnitId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit",
                column: "KillerUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit",
                column: "KillerUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillerUnitId",
                table: "Chat_SessionUnit");
        }
    }
}
