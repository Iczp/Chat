using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddProp_OwnerName_DestinationName_AppUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "Chat_SessionUnit",
                type: "uniqueidentifier",
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

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_DestinationName",
                table: "Chat_SessionUnit",
                column: "DestinationName",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_DestinationNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                column: "DestinationNameSpellingAbbreviation",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_MemberName",
                table: "Chat_SessionUnit",
                column: "MemberName",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                column: "MemberNameSpellingAbbreviation",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerName",
                table: "Chat_SessionUnit",
                column: "OwnerName",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerNameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                column: "OwnerNameSpellingAbbreviation",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Rename",
                table: "Chat_SessionUnit",
                column: "Rename",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_RenameSpellingAbbreviation",
                table: "Chat_SessionUnit",
                column: "RenameSpellingAbbreviation",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_DestinationName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_DestinationNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_MemberName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Rename",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_RenameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "DestinationName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "DestinationNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropColumn(
                name: "OwnerNameSpellingAbbreviation",
                table: "Chat_SessionUnit");
        }
    }
}
