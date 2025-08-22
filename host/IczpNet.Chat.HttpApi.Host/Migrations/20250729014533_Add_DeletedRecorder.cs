using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Add_DeletedRecorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc",
                table: "Chat_SessionUnit",
                newName: "IX_Chat_SessionUnit_$Sorting_Desc_$LastMessageId_Asc");

            migrationBuilder.CreateTable(
                name: "Chat_DeletedRecorder",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_DeletedRecorder", x => new { x.SessionUnitId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_Chat_DeletedRecorder_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_DeletedRecorder_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_DeletedRecorder_Chat_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_DeletedRecorder_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_Ticks_Asc",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "Ticks", "IsDeleted" },
                descending: new[] { true, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnit_Sorting_Ticks_IsDeleted",
                table: "Chat_SessionUnit",
                columns: new[] { "Sorting", "Ticks", "IsDeleted" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_DestinationId",
                table: "Chat_DeletedRecorder",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_MessageId",
                table: "Chat_DeletedRecorder",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_OwnerId",
                table: "Chat_DeletedRecorder",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_DeletedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_Ticks_Asc",
                table: "Chat_SessionUnit");

            migrationBuilder.DropIndex(
                name: "IX_Chat_SessionUnit_Sorting_Ticks_IsDeleted",
                table: "Chat_SessionUnit");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionUnit_$Sorting_Desc_$LastMessageId_Asc",
                table: "Chat_SessionUnit",
                newName: "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc");
        }
    }
}
