using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class AddCallModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_CallIdempotencyRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallIdempotencyRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_CallParticipant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    IsActiveBusy = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InviteRound = table.Column<int>(type: "int", nullable: false),
                    InviteNotifyState = table.Column<byte>(type: "tinyint", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DeviceSessionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LiveKitParticipantSid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RingExpireAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConnectExpireAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisconnectDeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastHeartbeatAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallParticipant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_CallRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallMode = table.Column<int>(type: "int", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    StateVersion = table.Column<long>(type: "bigint", nullable: false),
                    InitiatorUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InitiatorOwnerId = table.Column<long>(type: "bigint", nullable: false),
                    InitiatorDeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RoomEpoch = table.Column<int>(type: "int", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LiveKitRoomSid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BannerNotifyState = table.Column<byte>(type: "tinyint", nullable: false),
                    RingExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextCheckAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndReason = table.Column<int>(type: "int", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    HadScreenShare = table.Column<bool>(type: "bit", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    SummaryClientMessageId = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: false),
                    SummaryMessageId = table.Column<long>(type: "bigint", nullable: true),
                    SummaryDispatchStatus = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_CallRoomLifecycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomEpoch = table.Column<int>(type: "int", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LiveKitRoomSid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ActionState = table.Column<byte>(type: "tinyint", nullable: false),
                    RotationNotifyState = table.Column<byte>(type: "tinyint", nullable: false),
                    CloseReason = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreateDeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallRoomLifecycle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_CallWebhookEvent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RoomSid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_CallWebhookEvent", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_CallIdempotencyRecord");

            migrationBuilder.DropTable(
                name: "Chat_CallParticipant");

            migrationBuilder.DropTable(
                name: "Chat_CallRecord");

            migrationBuilder.DropTable(
                name: "Chat_CallRoomLifecycle");

            migrationBuilder.DropTable(
                name: "Chat_CallWebhookEvent");
        }
    }
}
