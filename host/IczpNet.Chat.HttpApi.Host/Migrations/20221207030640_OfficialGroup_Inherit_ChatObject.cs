using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class OfficialGroup_Inherit_ChatObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropIndex(
                name: "IX_Chat_OfficialGroup_OfficialId",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chat_OfficialGroup");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chat_OfficialGroup");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfficialId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "OfficialId",
                table: "Chat_OfficialGroup",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chat_OfficialGroup",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chat_OfficialGroup",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OfficialGroup_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OfficialGroup_Chat_Official_OfficialId",
                table: "Chat_OfficialGroup",
                column: "OfficialId",
                principalTable: "Chat_Official",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
