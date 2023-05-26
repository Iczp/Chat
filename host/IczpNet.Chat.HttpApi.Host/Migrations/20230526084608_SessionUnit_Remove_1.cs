using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_Remove_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Message_ReadedMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_DestinationName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_DestinationNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_IsPublic",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_IsStatic",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_MemberName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_MemberNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_DestinationId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerName",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerNameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_ReadedMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Rename",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_RenameSpellingAbbreviation",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId",
                table: "Chat_SessionUnit");

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
                name: "IX_Chat_SessionUnit_IsPublic",
                table: "Chat_SessionUnit",
                column: "IsPublic",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_IsStatic",
                table: "Chat_SessionUnit",
                column: "IsStatic",
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
                name: "IX_Chat_SessionUnit_OwnerId_DestinationId",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "DestinationId" },
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
                name: "IX_Chat_SessionUnit_ReadedMessageId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_SessionUnit_Chat_Message_ReadedMessageId",
                table: "Chat_SessionUnit",
                column: "ReadedMessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }
    }
}
