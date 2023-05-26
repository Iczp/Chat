using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class SessionUnitSetting_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_SessionUnitSetting",
                columns: table => new
                {
                    SessionUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadedMessageId = table.Column<long>(type: "bigint", nullable: true, comment: "已读的消息"),
                    HistoryFristTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "查看历史消息起始时间,为null时则不限"),
                    HistoryLastTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "查看历史消息截止时间,为null时则不限"),
                    IsKilled = table.Column<bool>(type: "bit", nullable: false, comment: "删除会话(退出群等)，但是不删除会话(用于查看历史消息)"),
                    KillType = table.Column<int>(type: "int", nullable: true),
                    KillTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "删除会话时间"),
                    KillerId = table.Column<long>(type: "bigint", nullable: true, comment: "删除人Id"),
                    ClearTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "清除历史消息最后时间,为null时则不限"),
                    RemoveTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "不显示消息会话(不退群,不删除消息)"),
                    MemberName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "会话内的名称"),
                    Rename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "备注名称"),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "备注其他"),
                    IsCantacts = table.Column<bool>(type: "bit", nullable: false, comment: "是否保存通讯录"),
                    IsImmersed = table.Column<bool>(type: "bit", nullable: false, comment: "消息免打扰，默认为 false"),
                    IsShowMemberName = table.Column<bool>(type: "bit", nullable: false, comment: "是否显示成员名称"),
                    IsShowReaded = table.Column<bool>(type: "bit", nullable: false, comment: "是否显示已读"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "创建时间"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "修改时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_SessionUnitSetting", x => x.SessionUnitId);
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitSetting_Chat_ChatObject_KillerId",
                        column: x => x.KillerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitSetting_Chat_Message_ReadedMessageId",
                        column: x => x.ReadedMessageId,
                        principalTable: "Chat_Message",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_SessionUnitSetting_Chat_SessionUnit_SessionUnitId",
                        column: x => x.SessionUnitId,
                        principalTable: "Chat_SessionUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_KillerId",
                table: "Chat_SessionUnitSetting",
                column: "KillerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SessionUnitSetting_ReadedMessageId",
                table: "Chat_SessionUnitSetting",
                column: "ReadedMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_SessionUnitSetting");
        }
    }
}
