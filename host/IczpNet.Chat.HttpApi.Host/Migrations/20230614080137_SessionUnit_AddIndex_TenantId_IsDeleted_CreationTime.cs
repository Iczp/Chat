using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_AddIndex_TenantId_IsDeleted_CreationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_CreationTime",
                table: "Chat_SessionUnit",
                column: "CreationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_IsDeleted",
                table: "Chat_SessionUnit",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted", "TenantId" },
                descending: new[] { true, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted_TenantId",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted", "TenantId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_TenantId",
                table: "Chat_SessionUnit",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_CreationTime",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted_TenantId",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_TenantId",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId" },
                descending: new bool[0]);
        }
    }
}
