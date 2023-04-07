using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_InviterUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InviterUnitId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit",
                column: "InviterUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit",
                column: "InviterUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "InviterUnitId",
                table: "Chat_SessionUnit");
        }
    }
}
