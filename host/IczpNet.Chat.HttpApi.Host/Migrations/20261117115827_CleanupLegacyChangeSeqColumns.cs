using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLegacyChangeSeqColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_ChangeSeq",
                table: "Chat_Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_DeletedRecorder_ChangeSeq",
                table: "Chat_DeletedRecorder");

            migrationBuilder.DropColumn(
                name: "ClearChangeSeq",
                table: "Chat_SessionUnitSetting");

            migrationBuilder.DropColumn(
                name: "ChangeSeq",
                table: "Chat_Message");

            migrationBuilder.DropColumn(
                name: "ChangeSeq",
                table: "Chat_DeletedRecorder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClearChangeSeq",
                table: "Chat_SessionUnitSetting",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChangeSeq",
                table: "Chat_Message",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChangeSeq",
                table: "Chat_DeletedRecorder",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_ChangeSeq",
                table: "Chat_Message",
                columns: new[] { "SessionId", "ChangeSeq" },
                filter: "[ChangeSeq] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeletedRecorder_ChangeSeq",
                table: "Chat_DeletedRecorder",
                columns: new[] { "SessionUnitId", "ChangeSeq" },
                filter: "[ChangeSeq] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "MessageId" });
        }
    }
}
