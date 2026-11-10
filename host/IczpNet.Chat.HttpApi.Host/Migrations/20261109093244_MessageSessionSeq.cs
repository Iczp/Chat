using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageSessionSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chat_AppVersionDevice");

            migrationBuilder.DropTable(
                name: "Chat_AppVersionDeviceGroup");

            migrationBuilder.DropTable(
                name: "Chat_DeviceGroupMap");

            migrationBuilder.DropTable(
                name: "Chat_UserDevice");

            migrationBuilder.DropTable(
                name: "Chat_AppVersion");

            migrationBuilder.DropTable(
                name: "Chat_DeviceGroup");

            migrationBuilder.DropTable(
                name: "Chat_Device");

            migrationBuilder.AddColumn<long>(
                name: "LastMessageSeq",
                table: "Chat_Session",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "当前会话最后分配的消息序号");

            migrationBuilder.AddColumn<long>(
                name: "SessionSeq",
                table: "Chat_Message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "当前消息在会话内的序号");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessageSeq",
                table: "Chat_Session");

            migrationBuilder.DropColumn(
                name: "SessionSeq",
                table: "Chat_Message");

            migrationBuilder.CreateTable(
                name: "Chat_AppVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsForce = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsWidget = table.Column<bool>(type: "bit", nullable: false),
                    IssueDate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PkgUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VersionCode = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_AppVersion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    App = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "当前运行的客户端"),
                    AppId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用appid"),
                    AppLanguage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "应用设置的语言"),
                    AppName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用名称"),
                    AppPlatform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "App 平台（可选）"),
                    AppVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "manifest.json 中应用版本名称"),
                    AppVersionCode = table.Column<long>(type: "bigint", nullable: false, comment: "manifest.json 中应用版本号"),
                    AppWgtVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "应用资源（wgt）的版本名称"),
                    BluetoothEnabled = table.Column<bool>(type: "bit", nullable: true, comment: "蓝牙开关状态（可选）"),
                    Brand = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "手机品牌"),
                    BrowserName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "浏览器名称"),
                    BrowserVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "浏览器版本/webview 版本"),
                    CacheLocation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "缓存的位置信息（可选）"),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceBrand = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备品牌"),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "设备 ID"),
                    DeviceModel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备型号"),
                    DeviceOrientation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备方向"),
                    DevicePixelRatio = table.Column<double>(type: "float", nullable: true, comment: "设备像素比"),
                    DeviceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备类型"),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSizeSetting = table.Column<double>(type: "float", nullable: true, comment: "用户字体大小设置"),
                    Host = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主平台"),
                    HostFontSizeSetting = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主字体大小设置"),
                    HostLanguage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主语言"),
                    HostName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主名称"),
                    HostPackageName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主包名"),
                    HostSDKVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主基础库版本"),
                    HostTheme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主主题"),
                    HostVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主版本"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "是否可用"),
                    Language = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "程序设置的语言"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocationEnabled = table.Column<bool>(type: "bit", nullable: true, comment: "定位开关状态（可选）"),
                    Model = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "手机型号"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "显示名称"),
                    NavigationBarHeight = table.Column<int>(type: "int", nullable: true, comment: "导航栏高度（可选）"),
                    OsLanguage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统语言"),
                    OsName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统名称"),
                    OsTheme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统主题"),
                    OsVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统版本"),
                    PixelRatio = table.Column<double>(type: "float", nullable: false, comment: "设备像素比"),
                    Platform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "客户端平台"),
                    Remarks = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "备注"),
                    RomName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "ROM 名称（Android 部分机型可能为空）"),
                    RomVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "ROM 版本号（Android 部分机型可能为空）"),
                    SDKVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "客户端基础库版本"),
                    SafeArea = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "屏幕高度"),
                    SafeAreaInsets = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "安全区域插入位置（可选）"),
                    ScreenHeight = table.Column<int>(type: "int", nullable: false, comment: "屏幕高度"),
                    ScreenWidth = table.Column<int>(type: "int", nullable: false, comment: "屏幕宽度"),
                    StatusBarHeight = table.Column<int>(type: "int", nullable: true, comment: "状态栏高度（可选）"),
                    Storage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "设备磁盘容量（可选）"),
                    SwanNativeVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "宿主平台版本号（可选）"),
                    System = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "操作系统信息"),
                    Theme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "系统主题（仅微信小程序支持，可选）"),
                    TitleBarHeight = table.Column<int>(type: "int", nullable: true, comment: "标题栏高度（可选）"),
                    Ua = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "用户标识（小程序为空）"),
                    UniCompileVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "uni 编译器版本号"),
                    UniPlatform = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "uni 运行平台（app/mp-weixin/web）"),
                    UniRuntimeVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "uni 运行时版本"),
                    Version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "引擎版本号"),
                    WifiEnabled = table.Column<bool>(type: "bit", nullable: true, comment: " Wi-Fi 开关状态（可选）"),
                    WindowBottom = table.Column<int>(type: "int", nullable: false, comment: "可用窗口底部位置"),
                    WindowHeight = table.Column<int>(type: "int", nullable: false, comment: "可用窗口高度"),
                    WindowTop = table.Column<int>(type: "int", nullable: false, comment: "可用窗口顶部位置"),
                    WindowWidth = table.Column<int>(type: "int", nullable: false, comment: "可用窗口宽度")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Device", x => x.Id);
                },
                comment: "设备");

            migrationBuilder.CreateTable(
                name: "Chat_DeviceGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_DeviceGroup", x => x.Id);
                },
                comment: "设备分组");

            migrationBuilder.CreateTable(
                name: "Chat_AppVersionDevice",
                columns: table => new
                {
                    AppVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_AppVersionDevice", x => new { x.AppVersionId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_Chat_AppVersionDevice_Chat_AppVersion_AppVersionId",
                        column: x => x.AppVersionId,
                        principalTable: "Chat_AppVersion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_AppVersionDeviceGroup",
                columns: table => new
                {
                    AppVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_AppVersionDeviceGroup", x => new { x.AppVersionId, x.DeviceGroupId });
                    table.ForeignKey(
                        name: "FK_Chat_AppVersionDeviceGroup_Chat_AppVersion_AppVersionId",
                        column: x => x.AppVersionId,
                        principalTable: "Chat_AppVersion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chat_UserDevice",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Abp User Id"),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: " Device Id"),
                    AppId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "AppId"),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RawDeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Raw DeviceId"),
                    RawDeviceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Raw DeviceType"),
                    UserName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "UserName")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_UserDevice", x => new { x.UserId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_Chat_UserDevice_Chat_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Chat_Device",
                        principalColumn: "Id");
                },
                comment: "用户设备");

            migrationBuilder.CreateTable(
                name: "Chat_DeviceGroupMap",
                columns: table => new
                {
                    DeviceGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_DeviceGroupMap", x => new { x.DeviceGroupId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_Chat_DeviceGroupMap_Chat_DeviceGroup_DeviceGroupId",
                        column: x => x.DeviceGroupId,
                        principalTable: "Chat_DeviceGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_DeviceGroupMap_Chat_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Chat_Device",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AppVersion_AppId_Platform_VersionCode",
                table: "Chat_AppVersion",
                columns: new[] { "AppId", "Platform", "VersionCode" },
                unique: true,
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AppVersion_VersionCode",
                table: "Chat_AppVersion",
                column: "VersionCode",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AppVersionDevice_DeviceId",
                table: "Chat_AppVersionDevice",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_AppVersionDeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup",
                column: "DeviceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Device_DeviceId",
                table: "Chat_Device",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Device_DeviceType",
                table: "Chat_Device",
                column: "DeviceType");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeviceGroup_Name",
                table: "Chat_DeviceGroup",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_DeviceGroupMap_DeviceId",
                table: "Chat_DeviceGroupMap",
                column: "DeviceId");

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
    }
}
