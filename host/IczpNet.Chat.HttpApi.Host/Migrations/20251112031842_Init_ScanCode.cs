using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Init_ScanCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_ScanCode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true, comment: "条码类型"),
                    Content = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, comment: "条码内容"),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "设备ID"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "用户Id"),
                    ClientId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "ClientId"),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ScanCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_ScanHandler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScanCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Handler = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "条码类型"),
                    Execution = table.Column<double>(type: "float", nullable: false, comment: "执行时间（毫秒）"),
                    Message = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "消息"),
                    Result = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true, comment: "处理结果"),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_ScanHandler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_ScanHandler_Chat_ScanCode_ScanCodeId",
                        column: x => x.ScanCodeId,
                        principalTable: "Chat_ScanCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ScanHandler_ScanCodeId",
                table: "Chat_ScanHandler",
                column: "ScanCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_ScanHandler");

            migrationBuilder.DropTable(
                name: "Chat_ScanCode");
        }
    }
}
