using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Message_AddIndex_CountQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_DeletedRecorder_MessageId",
                table: "Chat_DeletedRecorder");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "Chat_Device",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                comment: "设备 ID",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "设备 ID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_CountQuery",
                table: "Chat_Message",
                columns: new[] { "SessionId", "IsDeleted", "IsPrivate" })
                .Annotation("SqlServer:Include", new[] { "Id", "SenderSessionUnitId", "ReceiverSessionUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_MessageUnit",
                table: "Chat_DeletedRecorder",
                columns: new[] { "MessageId", "SessionUnitId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMessage_CountQuery",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_DeletedRecorder_MessageUnit",
                table: "Chat_DeletedRecorder");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "Chat_Device",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "设备 ID",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldComment: "设备 ID");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_MessageId",
                table: "Chat_DeletedRecorder",
                column: "MessageId");
        }
    }
}
