using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnit_FixIndex_OwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_$LastMessageId_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_Ticks_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Ticks_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_$OwnerId_$Sorting_Desc_$LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "Sorting", "LastMessageId", "IsDeleted" },
                descending: new[] { true, true, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_$OwnerId_$Sorting_Desc_Ticks_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "Sorting", "Ticks", "IsDeleted" },
                descending: new[] { true, true, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId_Sorting_LastMessageId_IsDeleted",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "Sorting", "LastMessageId", "IsDeleted" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_OwnerId_Sorting_Ticks_IsDeleted",
                table: "Chat_SessionUnit",
                columns: new[] { "OwnerId", "Sorting", "Ticks", "IsDeleted" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_$OwnerId_$Sorting_Desc_$LastMessageId_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_$OwnerId_$Sorting_Desc_Ticks_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_Sorting_LastMessageId_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_OwnerId_Sorting_Ticks_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_$LastMessageId_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted" },
                descending: new[] { true, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_Ticks_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "Ticks", "IsDeleted" },
                descending: new[] { true, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "LastMessageId", "IsDeleted" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Ticks_IsDeleted",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "Ticks", "IsDeleted" },
                descending: new bool[0]);
        }
    }
}
