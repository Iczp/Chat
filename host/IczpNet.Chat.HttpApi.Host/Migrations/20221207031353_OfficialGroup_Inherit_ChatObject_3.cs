using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class OfficialGroup_Inherit_ChatObject_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfficialId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId",
                principalTable: "Chat_Official",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfficialId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId",
                principalTable: "Chat_Official",
                principalColumn: "Id");
        }
    }
}
