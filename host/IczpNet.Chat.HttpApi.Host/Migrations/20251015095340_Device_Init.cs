using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class Device_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Platform",
                table: "Chat_ClientConfig",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "Platform",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "DeviceId");

            migrationBuilder.CreateTable(
                name: "Chat_Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "显示名称"),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "设备 ID"),
                    Platform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "客户端平台"),
                    AppId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用appid"),
                    SDKVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "客户端基础库版本"),
                    App = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "当前运行的客户端"),
                    AppLanguage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "应用设置的语言"),
                    AppName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用名称"),
                    AppVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用版本名称"),
                    AppVersionCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用版本号"),
                    AppWgtVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "应用资源（wgt）的版本名称"),
                    Brand = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "手机品牌"),
                    BrowserName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "浏览器名称"),
                    BrowserVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "浏览器版本/webview 版本"),
                    DeviceBrand = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备品牌"),
                    DeviceModel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备型号"),
                    DeviceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备类型"),
                    DevicePixelRatio = table.Column<double>(type: "float", nullable: true, comment: "设备像素比"),
                    DeviceOrientation = table.Column<int>(type: "int", nullable: true, comment: "设备方向"),
                    FontSizeSetting = table.Column<double>(type: "float", nullable: true, comment: "用户字体大小设置"),
                    Host = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主平台"),
                    HostFontSizeSetting = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主字体大小设置"),
                    HostSDKVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主基础库版本"),
                    HostName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主名称"),
                    HostVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主版本"),
                    HostLanguage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主语言"),
                    HostTheme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主主题"),
                    HostPackageName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主包名"),
                    Language = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "程序设置的语言"),
                    Model = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "手机型号"),
                    OsName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统名称"),
                    OsVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统版本"),
                    OsLanguage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统语言"),
                    OsTheme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统主题"),
                    PixelRatio = table.Column<double>(type: "float", nullable: false, comment: "设备像素比"),
                    ScreenWidth = table.Column<int>(type: "int", nullable: false, comment: "屏幕宽度"),
                    ScreenHeight = table.Column<int>(type: "int", nullable: false, comment: "屏幕高度"),
                    StatusBarHeight = table.Column<int>(type: "int", nullable: true, comment: "状态栏高度（可选）"),
                    Storage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备磁盘容量（可选）"),
                    SwanNativeVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主平台版本号（可选）"),
                    System = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统信息"),
                    SafeArea = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "屏幕高度"),
                    SafeAreaInsets = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "安全区域插入位置（可选）"),
                    Ua = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "用户标识（小程序为空）"),
                    UniCompileVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "uni 编译器版本号"),
                    UniPlatform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "uni 运行平台（app/mp-weixin/web）"),
                    UniRuntimeVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "uni 运行时版本"),
                    Version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "引擎版本号"),
                    RomName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "ROM 名称（Android 部分机型可能为空）"),
                    RomVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "ROM 版本号（Android 部分机型可能为空）"),
                    WindowWidth = table.Column<int>(type: "int", nullable: false, comment: "可用窗口宽度"),
                    WindowHeight = table.Column<int>(type: "int", nullable: false, comment: "可用窗口高度"),
                    NavigationBarHeight = table.Column<int>(type: "int", nullable: true, comment: "导航栏高度（可选）"),
                    TitleBarHeight = table.Column<int>(type: "int", maxLength: 64, nullable: true, comment: "标题栏高度（可选）"),
                    AppPlatform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "App 平台（可选）"),
                    WindowTop = table.Column<int>(type: "int", nullable: false, comment: "可用窗口顶部位置"),
                    WindowBottom = table.Column<int>(type: "int", nullable: false, comment: "可用窗口底部位置"),
                    BluetoothEnabled = table.Column<bool>(type: "bit", nullable: true, comment: "蓝牙开关状态（可选）"),
                    LocationEnabled = table.Column<bool>(type: "bit", nullable: true, comment: "定位开关状态（可选）"),
                    WifiEnabled = table.Column<bool>(type: "bit", nullable: true, comment: " Wi-Fi 开关状态（可选）"),
                    CacheLocation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "缓存的位置信息（可选）"),
                    Theme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "系统主题（仅微信小程序支持，可选）"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "是否可用"),
                    Remarks = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "备注"),
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
                    table.PrimaryKey("PK_Chat_Device", x => x.Id);
                },
                comment: "设备");

            migrationBuilder.CreateTable(
                name: "Chat_UserDevice",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Abp User Id"),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: " Device Id"),
                    RawDeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Raw DeviceId"),
                    RawDeviceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Raw DeviceType"),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_UserDevice", x => new { x.UserId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_Chat_UserDevice_Chat_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Chat_Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "用户设备");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Device_DeviceId",
                table: "Chat_Device",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Device_DeviceType",
                table: "Chat_Device",
                column: "DeviceType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UserDevice_DeviceId",
                table: "Chat_UserDevice",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UserDevice_RawDeviceType",
                table: "Chat_UserDevice",
                column: "RawDeviceType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_UserDevice_UserId",
                table: "Chat_UserDevice",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_UserDevice");

            migrationBuilder.DropTable(
                name: "Chat_Device");

            migrationBuilder.AlterColumn<string>(
                name: "Platform",
                table: "Chat_ClientConfig",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "DeviceId",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "Platform");
        }
    }
}
