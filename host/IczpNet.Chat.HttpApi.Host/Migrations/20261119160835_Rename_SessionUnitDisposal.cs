using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Rename_SessionUnitDisposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitDisposalRecord");

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitDisposal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false, comment: "所属聊天对象ID"),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话单元ID"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "所属会话ID"),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true, comment: "对方聊天对象ID"),
                    DisposalType = table.Column<byte>(type: "tinyint", nullable: false, comment: "处置动作类型: 1=Remove, 2=Delete, 3=Hide, 4=ClearMessage"),
                    Reason = table.Column<byte>(type: "tinyint", nullable: false, comment: "处置原因细分"),
                    OperatorId = table.Column<long>(type: "bigint", nullable: true, comment: "操作执行人ID"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "附加说明"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitDisposal", x => x.Id);
                },
                comment: "会话单元处置与删除记录");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitDisposal_CreationTime",
                table: "Chat_SessionUnitDisposal",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitDisposal_OwnerId_Id",
                table: "Chat_SessionUnitDisposal",
                columns: new[] { "OwnerId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitDisposal");

            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitDisposalRecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "附加说明"),
                    DestinationId = table.Column<long>(type: "bigint", nullable: true, comment: "对方聊天对象ID"),
                    DisposalType = table.Column<byte>(type: "tinyint", nullable: false, comment: "处置动作类型: 1=Remove, 2=Delete, 3=Hide, 4=ClearMessage"),
                    OperatorId = table.Column<long>(type: "bigint", nullable: true, comment: "操作执行人ID"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false, comment: "所属聊天对象ID"),
                    Reason = table.Column<byte>(type: "tinyint", nullable: false, comment: "处置原因细分"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "所属会话ID"),
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "会话单元ID"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitDisposalRecord", x => x.Id);
                },
                comment: "会话单元处置与删除记录");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitDisposalRecord_CreationTime",
                table: "Chat_SessionUnitDisposalRecord",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitDisposalRecord_OwnerId_Id",
                table: "Chat_SessionUnitDisposalRecord",
                columns: new[] { "OwnerId", "Id" });
        }
    }
}
