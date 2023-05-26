using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_Remove_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_InviterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_KillerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_InviterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_KillerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "ClearTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "DestinationName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "DestinationNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "HistoryFristTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "HistoryLastTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "InviterId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "InviterUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsCantacts",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsImmersed",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsImportant",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsKilled",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsShowMemberName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "IsShowReaded",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillType",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillerId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "KillerUnitId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "OwnerNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "RemoveTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "Rename",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "RenameSpelling",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImage",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JoinWay",
                table: "Chat_SessionUnitSetting",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenameSpelling",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImage",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "JoinWay",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "RenameSpelling",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClearTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationName",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HistoryFristTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HistoryLastTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InviterId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InviterUnitId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCantacts",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImmersed",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImportant",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsKilled",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShowMemberName",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShowReaded",
                table: "Chat_SessionUnit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "KillTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KillType",
                table: "Chat_SessionUnit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "KillerId",
                table: "Chat_SessionUnit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KillerUnitId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberNameSpelling",
                table: "Chat_SessionUnit",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "Chat_SessionUnit",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemoveTime",
                table: "Chat_SessionUnit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rename",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenameSpelling",
                table: "Chat_SessionUnit",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_InviterId",
                table: "Chat_SessionUnit",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit",
                column: "InviterUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_KillerId",
                table: "Chat_SessionUnit",
                column: "KillerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit",
                column: "KillerUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_InviterId",
                table: "Chat_SessionUnit",
                column: "InviterId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_ChatObject_KillerId",
                table: "Chat_SessionUnit",
                column: "KillerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_InviterUnitId",
                table: "Chat_SessionUnit",
                column: "InviterUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_SessionUnit_KillerUnitId",
                table: "Chat_SessionUnit",
                column: "KillerUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id");
        }
    }
}
