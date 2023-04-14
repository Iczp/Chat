using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionRequest_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    RequestMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsHandled = table.Column<bool>(type: "bit", nullable: false),
                    IsAgreed = table.Column<bool>(type: "bit", nullable: true),
                    HandleMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HandleTime = table.Column<DateTime>(type: "datetime2", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HandlerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_SessionRequest_Chat_ChatObject_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionRequest_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_SessionRequest_Chat_SessionUnit_HandlerId",
                        column: x => x.HandlerId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_DestinationId",
                table: "Chat_SessionRequest",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_HandlerId",
                table: "Chat_SessionRequest",
                column: "HandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionRequest_OwnerId",
                table: "Chat_SessionRequest",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionRequest");
        }
    }
}
