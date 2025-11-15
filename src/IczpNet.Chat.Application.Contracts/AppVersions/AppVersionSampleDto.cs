using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.AppVersions;

/// <summary>
/// SampleDto
/// </summary>
[Serializable]
public class AppVersionSampleDto : BaseDto<Guid>
{
    /// <summary>
    /// 版本号
    /// </summary>
    public virtual string Version { get; set; }

    /// <summary>
    /// 版本编码(升级主要依据)
    /// </summary>
    public virtual long VersionCode { get; set; }

    /// <summary>
    /// 应用ID
    /// </summary>
    public virtual string AppId { get; set; }

    /// <summary>
    /// 适用平台 ios | android | electron
    /// </summary>
    public virtual string Platform { get; set; }

    /// <summary>
    /// 标题（更新日志）
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// 下载页面(Web)
    /// </summary>
    public virtual string PageUrl { get; set; }

    /// <summary>
    /// 包下载址
    /// </summary>
    public virtual string PkgUrl { get; set; }
    /// <summary>
    /// 
    /// 发行日期
    /// </summary>

    public virtual string IssueDate { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public virtual bool IsForce { get; set; }

    /// <summary>
    /// 是否热更新包(.wgt)
    /// </summary>
    public virtual bool IsWidget { get; set; }

    /// <summary>
    /// 公开，则全部可以使用，否则只有指定的分组可以使用
    /// </summary>
    public virtual bool IsPublic { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsEnabled { get; set; }

}