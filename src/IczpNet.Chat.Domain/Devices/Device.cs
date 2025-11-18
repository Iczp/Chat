using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.DeviceGroupMaps;
using IczpNet.Chat.DeviceGroups;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.Devices;

/// <summary>
/// 设备
/// </summary>
[Index(nameof(DeviceId))]
[Index(nameof(DeviceType))]
[Comment("设备")]
public class Device : BaseEntity<Guid>, IDeviceId, IIsEnabled
{
    /// <summary>
    /// 显示名称
    /// </summary>
    [Comment("显示名称")]
    [StringLength(128)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 设备 ID
    /// </summary>
    [Comment("设备 ID")]
    [StringLength(128)]
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// 客户端平台
    /// </summary>
    [Comment("客户端平台")]
    [StringLength(64)]
    public virtual string Platform { get; set; }

    /// <summary>
    /// manifest.json 中应用appid
    /// </summary>
    [Comment("manifest.json 中应用appid")]
    [StringLength(64)]
    public virtual string AppId { get; set; }

    /// <summary>
    /// 客户端基础库版本
    /// </summary>
    [Comment("客户端基础库版本")]
    [StringLength(64)]
    public virtual string SDKVersion { get; set; }

    /// <summary>
    /// 当前运行的客户端（可选）
    /// </summary>
    [Comment("当前运行的客户端")]
    [StringLength(64)]
    public virtual string App { get; set; }

    /// <summary>
    /// 应用设置的语言（仅 App、H5 支持，可选）
    /// </summary>
    [Comment("应用设置的语言")]
    [StringLength(64)]
    public virtual string AppLanguage { get; set; }

    /// <summary>
    /// manifest.json 中应用名称
    /// </summary>
    [Comment("manifest.json 中应用名称")]
    [StringLength(64)]
    public virtual string AppName { get; set; }

    /// <summary>
    /// manifest.json 中应用版本名称
    /// </summary>
    [Comment("manifest.json 中应用版本名称")]
    [StringLength(64)]
    public virtual string AppVersion { get; set; }

    /// <summary>
    /// manifest.json 中应用版本号
    /// </summary>
    [Comment("manifest.json 中应用版本号")]
    //[StringLength(64)]
    public virtual long AppVersionCode { get; set; }

    /// <summary>
    /// 应用资源（wgt）的版本名称（仅 App 支持，可选）
    /// </summary>
    [Comment("应用资源（wgt）的版本名称")]
    [StringLength(64)]
    public virtual string AppWgtVersion { get; set; }

    /// <summary>
    /// 手机品牌（H5 不支持，可选）
    /// </summary>
    [Comment("手机品牌")]
    [StringLength(64)]
    public virtual string Brand { get; set; }

    /// <summary>
    /// 浏览器名称
    /// </summary>
    [Comment("浏览器名称")]
    [StringLength(64)]
    public virtual string BrowserName { get; set; }

    /// <summary>
    /// 浏览器版本/webview 版本
    /// </summary>
    [Comment("浏览器版本/webview 版本")]
    [StringLength(64)]
    public virtual string BrowserVersion { get; set; }


    /// <summary>
    /// 设备品牌（H5 不支持，可选）
    /// </summary>
    [Comment("设备品牌")]
    [StringLength(64)]
    public virtual string DeviceBrand { get; set; }

    /// <summary>
    /// 设备型号
    /// </summary>
    [Comment("设备型号")]
    [StringLength(64)]
    public virtual string DeviceModel { get; set; }

    /// <summary>
    /// 设备类型（phone/pad/pc/web/html5plus）
    /// </summary>
    [Comment("设备类型")]
    [StringLength(64)]
    public virtual string DeviceType { get; set; }

    // 其他非string类型属性（保持可空但移除StringLength）
    [Comment("设备像素比")]
    public virtual double? DevicePixelRatio { get; set; }

    /// <summary>
    /// 竖屏: Portrait, 横屏:Landscape
    /// </summary>
    [Comment("设备方向")]
    [StringLength(64)]
    public virtual string DeviceOrientation { get; set; }

    /// <summary>
    /// 用户字体大小设置
    /// </summary>
    [Comment("用户字体大小设置")]
    public virtual double? FontSizeSetting { get; set; }

    /// <summary>
    /// 宿主平台（可选）
    /// </summary>
    [Comment("宿主平台")]
    [StringLength(64)]
    public virtual string Host { get; set; }

    /// <summary>
    /// 宿主字体大小设置（可选）
    /// </summary>
    [Comment("宿主字体大小设置")]
    [StringLength(64)]
    public virtual string HostFontSizeSetting { get; set; }

    /// <summary>
    /// 宿主基础库版本（可选）
    /// </summary>
    [Comment("宿主基础库版本")]
    [StringLength(64)]
    public virtual string HostSDKVersion { get; set; }

    /// <summary>
    /// 宿主名称（App/小程序，可选）
    /// </summary>
    [Comment("宿主名称")]
    [StringLength(64)]
    public virtual string HostName { get; set; }

    /// <summary>
    /// 宿主版本（App/小程序，可选）
    /// </summary>
    [Comment("宿主版本")]
    [StringLength(64)]
    public virtual string HostVersion { get; set; }

    /// <summary>
    /// 宿主语言（小程序/App，可选）
    /// </summary>
    [Comment("宿主语言")]
    [StringLength(64)]
    public virtual string HostLanguage { get; set; }

    /// <summary>
    /// 宿主主题（App/小程序，可选）
    /// </summary>
    [Comment("宿主主题")]
    [StringLength(64)]
    public virtual string HostTheme { get; set; }

    /// <summary>
    /// 宿主包名（仅 App，可选）
    /// </summary>
    [Comment("宿主包名")]
    [StringLength(64)]
    public virtual string HostPackageName { get; set; }

    /// <summary>
    /// 程序设置的语言（可选）
    /// </summary>
    [Comment("程序设置的语言")]
    [StringLength(64)]
    public virtual string Language { get; set; }

    /// <summary>
    /// 手机型号
    /// </summary>
    [Comment("手机型号")]
    [StringLength(64)]
    public virtual string Model { get; set; }

    /// <summary>
    /// 操作系统名称（ios/android/windows/mac/linux）
    /// </summary>
    [Comment("操作系统名称")]
    [StringLength(64)]
    public virtual string OsName { get; set; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    [Comment("操作系统版本")]
    [StringLength(64)]
    public virtual string OsVersion { get; set; }

    /// <summary>
    /// 操作系统语言（可选）
    /// </summary>
    [Comment("操作系统语言")]
    [StringLength(64)]
    public virtual string OsLanguage { get; set; }

    /// <summary>
    /// 操作系统主题（可选）
    /// </summary>
    [Comment("操作系统主题")]
    [StringLength(64)]
    public virtual string OsTheme { get; set; }

    /// <summary>
    /// 设备像素比
    /// </summary>
    [Comment("设备像素比")]
    public virtual double PixelRatio { get; set; }

    /// <summary>
    /// 屏幕宽度
    /// </summary>
    [Comment("屏幕宽度")]
    public virtual int ScreenWidth { get; set; }

    /// <summary>
    /// 屏幕高度
    /// </summary>
    [Comment("屏幕高度")]
    public virtual int ScreenHeight { get; set; }

    /// <summary>
    /// 状态栏高度（可选）
    /// </summary>
    [Comment("状态栏高度（可选）")]
    public virtual int? StatusBarHeight { get; set; }

    /// <summary>
    /// 设备磁盘容量（可选）
    /// </summary>
    [StringLength(64)]
    [Comment("设备磁盘容量（可选）")]
    public virtual string Storage { get; set; }

    /// <summary>
    /// 宿主平台版本号（可选）
    /// </summary>
    [StringLength(64)]
    [Comment("宿主平台版本号（可选）")]
    public virtual string SwanNativeVersion { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    [StringLength(64)]
    [Comment("操作系统信息")]
    public virtual string System { get; set; }

    /// <summary>
    /// 安全区域信息（可选）
    /// </summary>
    [StringLength(500)]
    [Comment("屏幕高度")]
    public virtual string SafeArea { get; set; }

    /// <summary>
    /// 安全区域插入位置（可选）
    /// </summary>
    [StringLength(64)]
    [Comment("安全区域插入位置（可选）")]
    public virtual string SafeAreaInsets { get; set; }

    /// <summary>
    /// 用户标识（小程序为空）
    /// </summary>
    [StringLength(512)]
    [Comment("用户标识（小程序为空）")]
    public virtual string Ua { get; set; }

    /// <summary>
    /// uni 编译器版本号
    /// </summary>
    [StringLength(64)]
    [Comment("uni 编译器版本号")]
    public virtual string UniCompileVersion { get; set; }

    /// <summary>
    /// uni 运行平台（app/mp-weixin/web）
    /// </summary>
    [StringLength(64)]
    [Comment("uni 运行平台（app/mp-weixin/web）")]
    public virtual string UniPlatform { get; set; }

    /// <summary>
    /// uni 运行时版本
    /// </summary>
    [StringLength(64)]
    [Comment("uni 运行时版本")]
    public virtual string UniRuntimeVersion { get; set; }

    /// <summary>
    /// 引擎版本号
    /// </summary>
    [StringLength(64)]
    [Comment("引擎版本号")]
    public virtual string Version { get; set; }

    /// <summary>
    /// ROM 名称（Android 部分机型可能为空）
    /// </summary>
    [StringLength(64)]
    [Comment("ROM 名称（Android 部分机型可能为空）")]
    public virtual string RomName { get; set; }

    /// <summary>
    /// ROM 版本号（Android 部分机型可能为空）
    /// </summary>
    [StringLength(64)]
    [Comment("ROM 版本号（Android 部分机型可能为空）")]
    public virtual string RomVersion { get; set; }

    /// <summary>
    /// 可用窗口宽度
    /// </summary>
    [Comment("可用窗口宽度")]
    public virtual int WindowWidth { get; set; }

    /// <summary>
    /// 可用窗口高度
    /// </summary>
    [Comment("可用窗口高度")]
    public virtual int WindowHeight { get; set; }

    /// <summary>
    /// 导航栏高度（可选）
    /// </summary>
    [Comment("导航栏高度（可选）")]
    public virtual int? NavigationBarHeight { get; set; }

    /// <summary>
    /// 标题栏高度（可选）
    /// </summary>
    [Comment("标题栏高度（可选）")]
    public virtual int? TitleBarHeight { get; set; }

    ///// <summary>
    ///// 当前电量百分比（可选）
    ///// </summary>
    //[StringLength(64)]
    //[Comment("当前电量百分比（可选）")]
    //public virtual string CurrentBattery { get; set; }

    /// <summary>
    /// App 平台（可选）
    /// </summary>
    [StringLength(64)]
    [Comment("App 平台（可选）")]
    public virtual string AppPlatform { get; set; }

    /// <summary>
    /// 可用窗口顶部位置
    /// </summary>
    [Comment("可用窗口顶部位置")]
    public virtual int WindowTop { get; set; }

    /// <summary>
    /// 可用窗口底部位置
    /// </summary>
    [Comment("可用窗口底部位置")]
    public virtual int WindowBottom { get; set; }

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
    [Comment("蓝牙开关状态（可选）")]
    public virtual bool? BluetoothEnabled { get; set; }

    /// <summary>
    /// 定位开关状态（可选）
    /// </summary>
    [Comment("定位开关状态（可选）")]
    public virtual bool? LocationEnabled { get; set; }

    /// <summary>
    /// Wi-Fi 开关状态（可选）
    /// </summary>
    [Comment(" Wi-Fi 开关状态（可选）")]
    public virtual bool? WifiEnabled { get; set; }

    /// <summary>
    /// 缓存的位置信息（可选）
    /// </summary>
    [StringLength(64)]
    [Comment("缓存的位置信息（可选）")]
    public virtual string CacheLocation { get; set; }

    /// <summary>
    /// 系统主题（仅微信小程序支持，可选）
    /// </summary>
    [StringLength(64)]
    [Comment("系统主题（仅微信小程序支持，可选）")]
    public virtual string Theme { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    [Comment("是否可用")]
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [Comment("备注")]
    [StringLength(256)]
    public virtual string Remarks { get; set; }

    /// <summary>
    /// UserDeviceList
    /// </summary>
    public virtual IList<UserDevice> UserDeviceList { get; protected set; } = [];

    /// <summary>
    /// DeviceGroupMapList
    /// </summary>
    public virtual IList<DeviceGroupMap> DeviceGroupMapList { get; protected set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual IList<DeviceGroup> Groups => DeviceGroupMapList.Where(x => !x.DeviceGroup.IsDeleted).Select(x => x.DeviceGroup).ToList();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="groupIdList"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public int SetGroups(List<Guid> groupIdList)
    {
        DeviceGroupMapList.Clear();
        DeviceGroupMapList = groupIdList.Select(x => new DeviceGroupMap()
        {
            DeviceId = Id,
            DeviceGroupId = x

        }).ToList();

        return groupIdList.Count;
    }
}
