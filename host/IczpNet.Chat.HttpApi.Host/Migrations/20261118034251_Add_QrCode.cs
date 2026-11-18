using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Add_QrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chat_QrCode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "唯一短码"),
                    Scene = table.Column<int>(type: "int", nullable: false, comment: "二维码场景类型"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "标题"),
                    Content = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, comment: "二维码内容"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true, comment: "所属聊天对象Id"),
                    OwnerObjectType = table.Column<int>(type: "int", nullable: true, comment: "所属聊天对象类型"),
                    InviterId = table.Column<long>(type: "bigint", nullable: true, comment: "邀请人聊天对象Id"),
                    IsPermanent = table.Column<bool>(type: "bit", nullable: false, comment: "是否永久有效"),
                    ExpireDays = table.Column<int>(type: "int", nullable: true, comment: "有效天数"),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "过期时间"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "是否可用"),
                    MaxUseCount = table.Column<int>(type: "int", nullable: true, comment: "最大使用次数"),
                    UsedCount = table.Column<int>(type: "int", nullable: false, comment: "已使用次数"),
                    PayloadJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "扩展业务载荷JSON"),
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
                    table.PrimaryKey("PK_Chat_QrCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_QrCode_Chat_ChatObject_InviterId",
                        column: x => x.InviterId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chat_QrCode_Chat_ChatObject_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Chat_ChatObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_QrCode_Code",
                table: "Chat_QrCode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_QrCode_ExpirationTime",
                table: "Chat_QrCode",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_QrCode_InviterId",
                table: "Chat_QrCode",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_QrCode_OwnerId_Scene_IsPermanent_IsEnabled",
                table: "Chat_QrCode",
                columns: new[] { "OwnerId", "Scene", "IsPermanent", "IsEnabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_QrCode");
        }
    }
}
