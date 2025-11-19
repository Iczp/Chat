using System; 
namespace IczpNet.Chat.Devices; 
///<summary>
/// 详情 
///</summary>
[Serializable] 
public class DeviceDetailDto : DeviceDto {


    /// <summary>
    /// 当前运行的客户端（可选）
    /// </summary>
    public virtual string App { get; set; }

    /// <summary>
    /// 应用设置的语言（仅 App、H5 支持，可选）
    /// </summary>
    public virtual string AppLanguage { get; set; }

    /// <summary>
    /// manifest.json 中应用名称
    /// </summary>
    public virtual string AppName { get; set; }

    /// <summary>
    /// manifest.json 中应用版本名称
    /// </summary>
    public virtual string AppVersion { get; set; }

    /// <summary>
    /// manifest.json 中应用版本号
    /// </summary>
    public virtual long AppVersionCode { get; set; }

    /// <summary>
    /// 应用资源（wgt）的版本名称（仅 App 支持，可选）
    /// </summary>
    public virtual string AppWgtVersion { get; set; }

    /// <summary>
    /// 客户端基础库版本
    /// </summary>
    public virtual string SDKVersion { get; set; }

    /// <summary>
    /// 可用窗口顶部位置
    /// </summary>
    public virtual int WindowTop { get; set; }

    /// <summary>
    /// 可用窗口底部位置
    /// </summary>
    public virtual int WindowBottom { get; set; }


    /// <summary>
    /// 浏览器名称
    /// </summary>
    public virtual string BrowserName { get; set; }

    /// <summary>
    /// 浏览器版本/webview 版本
    /// </summary>
    public virtual string BrowserVersion { get; set; }


    // 其他非string类型属性（保持可空但移除MaxLength）
    public virtual double? DevicePixelRatio { get; set; }

    /// <summary>
    /// 竖屏: Portrait, 横屏:Landscape
    /// </summary>
    public virtual string DeviceOrientation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual double? FontSizeSetting { get; set; }

    /// <summary>
    /// 宿主平台（可选）
    /// </summary>
    public virtual string Host { get; set; }

    /// <summary>
    /// 宿主字体大小设置（可选）
    /// </summary>
    public virtual string HostFontSizeSetting { get; set; }

    /// <summary>
    /// 宿主基础库版本（可选）
    /// </summary>
    public virtual string HostSDKVersion { get; set; }

    /// <summary>
    /// 宿主名称（App/小程序，可选）
    /// </summary>
    public virtual string HostName { get; set; }

    /// <summary>
    /// 宿主版本（App/小程序，可选）
    /// </summary>
    public virtual string HostVersion { get; set; }

    /// <summary>
    /// 宿主语言（小程序/App，可选）
    /// </summary>
    public virtual string HostLanguage { get; set; }

    /// <summary>
    /// 宿主主题（App/小程序，可选）
    /// </summary>
    public virtual string HostTheme { get; set; }

    /// <summary>
    /// 宿主包名（仅 App，可选）
    /// </summary>
    public virtual string HostPackageName { get; set; }

    /// <summary>
    /// 程序设置的语言（可选）
    /// </summary>
    public virtual string Language { get; set; }


    /// <summary>
    /// 操作系统名称（ios/android/windows/mac/linux）
    /// </summary>
    public virtual string OsName { get; set; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    public virtual string OsVersion { get; set; }

    /// <summary>
    /// 操作系统语言（可选）
    /// </summary>
    public virtual string OsLanguage { get; set; }

    /// <summary>
    /// 操作系统主题（可选）
    /// </summary>
    public virtual string OsTheme { get; set; }

    /// <summary>
    /// 设备像素比
    /// </summary>
    public virtual double PixelRatio { get; set; }

    /// <summary>
    /// 屏幕宽度
    /// </summary>
    public virtual int ScreenWidth { get; set; }

    /// <summary>
    /// 屏幕高度
    /// </summary>
    public virtual int ScreenHeight { get; set; }

    /// <summary>
    /// 状态栏高度（可选）
    /// </summary>
    public virtual int? StatusBarHeight { get; set; }

    /// <summary>
    /// 设备磁盘容量（可选）
    /// </summary>
    public virtual string Storage { get; set; }

    /// <summary>
    /// 宿主平台版本号（可选）
    /// </summary>
    public virtual string SwanNativeVersion { get; set; }


    ///// <summary>
    ///// 相册权限（iOS 仅有效，可选）
    ///// </summary>
    //[Comment("相册权限（iOS 仅有效，可选）")] 
    //public virtual bool? AlbumAuthorized { get; set; }

    ///// <summary>
    ///// 摄像头权限
    ///// </summary>
    //[Comment("摄像头权限")] 
    //public virtual bool? CameraAuthorized { get; set; }

    ///// <summary>
    ///// 定位权限
    ///// </summary>
    //[Comment("定位权限")] 
    //public virtual bool? LocationAuthorized { get; set; }

    ///// <summary>
    ///// 麦克风权限
    ///// </summary>
    //[Comment("麦克风权限")] 
    //public virtual bool? MicrophoneAuthorized { get; set; }

    ///// <summary>
    ///// 通知权限
    ///// </summary>
    //[Comment("通知权限")]
    //public virtual bool? NotificationAuthorized { get; set; }

    ///// <summary>
    ///// 通知提醒权限（iOS 仅有效，可选）
    ///// </summary>
    //[Comment("通知提醒权限（iOS 仅有效，可选）")]
    //public virtual bool? NotificationAlertAuthorized { get; set; }

    ///// <summary>
    ///// 通知标记权限（iOS 仅有效，可选）
    ///// </summary>
    //[Comment(" 通知标记权限（iOS 仅有效，可选）")]
    //public virtual bool? NotificationBadgeAuthorized { get; set; }

    ///// <summary>
    ///// 通知声音权限（iOS 仅有效，可选）
    ///// </summary>
    //[Comment("通知声音权限（iOS 仅有效，可选）")]
    //public virtual bool? NotificationSoundAuthorized { get; set; }

    /// <summary>
    /// 蓝牙开关状态（可选）
    /// </summary>

    public virtual bool? BluetoothEnabled { get; set; }

    /// <summary>
    /// 定位开关状态（可选）
    /// </summary>
    public virtual bool? LocationEnabled { get; set; }

    /// <summary>
    /// Wi-Fi 开关状态（可选）
    /// </summary>
    public virtual bool? WifiEnabled { get; set; }

    /// <summary>
    /// 缓存的位置信息（可选）
    /// </summary>
    public virtual string CacheLocation { get; set; }

    /// <summary>
    /// 系统主题（仅微信小程序支持，可选）
    /// </summary>
    public virtual string Theme { get; set; }

    /// <summary>
    /// 安全区域信息（可选）
    /// </summary>
    public virtual string SafeArea { get; set; }

    /// <summary>
    /// 安全区域插入位置（可选）
    /// </summary>
    public virtual string SafeAreaInsets { get; set; }

    /// <summary>
    /// 用户标识（小程序为空）
    /// </summary>
    public virtual string Ua { get; set; }

    /// <summary>
    /// uni 编译器版本号
    /// </summary>
    public virtual string UniCompileVersion { get; set; }

    /// <summary>
    /// uni 运行平台（app/mp-weixin/web）
    /// </summary>
    public virtual string UniPlatform { get; set; }

    /// <summary>
    /// uni 运行时版本
    /// </summary>
    public virtual string UniRuntimeVersion { get; set; }

    /// <summary>
    /// 引擎版本号
    /// </summary>
    public virtual string Version { get; set; }

    /// <summary>
    /// ROM 名称（Android 部分机型可能为空）
    /// </summary>
    public virtual string RomName { get; set; }

    /// <summary>
    /// ROM 版本号（Android 部分机型可能为空）
    /// </summary>
    public virtual string RomVersion { get; set; }

    /// <summary>
    /// 可用窗口宽度
    /// </summary>
    public virtual int WindowWidth { get; set; }

    /// <summary>
    /// 可用窗口高度
    /// </summary>
    public virtual int WindowHeight { get; set; }

    /// <summary>
    /// 导航栏高度（可选）
    /// </summary>
    public virtual int? NavigationBarHeight { get; set; }

    /// <summary>
    /// 标题栏高度（可选）
    /// </summary>
    public virtual int? TitleBarHeight { get; set; }

    ///// <summary>
    ///// 当前电量百分比（可选）
    ///// </summary>
    //[MaxLength(64)]
    //[Comment("当前电量百分比（可选）")]
    //public virtual string CurrentBattery { get; set; }

    /// <summary>
    /// App 平台（可选）
    /// </summary>
    public virtual string AppPlatform { get; set; }
}