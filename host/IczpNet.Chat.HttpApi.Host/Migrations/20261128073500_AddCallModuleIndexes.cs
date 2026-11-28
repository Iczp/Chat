using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using IczpNet.Chat.EntityFrameworkCore;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    [DbContext(typeof(ChatHttpApiHostMigrationsDbContext))]
    [Migration("20261128073500_AddCallModuleIndexes")]
    public partial class AddCallModuleIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Chat_CallRecord_ActiveGroupSession",
                table: "Chat_CallRecord",
                column: "SessionId",
                unique: true,
                filter: "[CallMode] = 2 AND [State] <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallRecord_State_NextCheckAt",
                table: "Chat_CallRecord",
                columns: new[] { "State", "NextCheckAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallRecord_SummaryClientMessageId",
                table: "Chat_CallRecord",
                column: "SummaryClientMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallRecord_SummaryMessageId",
                table: "Chat_CallRecord",
                column: "SummaryMessageId",
                unique: true,
                filter: "[SummaryMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallParticipant_CallId_SessionUnitId",
                table: "Chat_CallParticipant",
                columns: new[] { "CallId", "SessionUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Chat_CallParticipant_ActiveOwner",
                table: "Chat_CallParticipant",
                column: "OwnerId",
                unique: true,
                filter: "[IsActiveBusy] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallRoomLifecycle_CallId_RoomEpoch",
                table: "Chat_CallRoomLifecycle",
                columns: new[] { "CallId", "RoomEpoch" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallRoomLifecycle_ActionState_NextRetryAt",
                table: "Chat_CallRoomLifecycle",
                columns: new[] { "ActionState", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CallIdempotencyRecord_SessionUnitId_DeviceId_Action_RequestId",
                table: "Chat_CallIdempotencyRecord",
                columns: new[] { "SessionUnitId", "DeviceId", "Action", "RequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Chat_CallRecord_ActiveGroupSession",
                table: "Chat_CallRecord");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallRecord_State_NextCheckAt",
                table: "Chat_CallRecord");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallRecord_SummaryClientMessageId",
                table: "Chat_CallRecord");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallRecord_SummaryMessageId",
                table: "Chat_CallRecord");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallParticipant_CallId_SessionUnitId",
                table: "Chat_CallParticipant");

            migrationBuilder.DropIndex(
                name: "UX_Chat_CallParticipant_ActiveOwner",
                table: "Chat_CallParticipant");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallRoomLifecycle_CallId_RoomEpoch",
                table: "Chat_CallRoomLifecycle");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallRoomLifecycle_ActionState_NextRetryAt",
                table: "Chat_CallRoomLifecycle");

            migrationBuilder.DropIndex(
                name: "IX_Chat_CallIdempotencyRecord_SessionUnitId_DeviceId_Action_RequestId",
                table: "Chat_CallIdempotencyRecord");
        }
    }
}
