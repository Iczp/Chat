using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitCounter_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitCounter",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicBadge = table.Column<int>(type: "int", nullable: false),
                    PrivateBadge = table.Column<int>(type: "int", nullable: false),
                    RemindAllCount = table.Column<int>(type: "int", nullable: false),
                    RemindMeCount = table.Column<int>(type: "int", nullable: false),
                    FollowingCount = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitCounter", x => x.SessionUnitId);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitCounter_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitCounter");
        }
    }
}
