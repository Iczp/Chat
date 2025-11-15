using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace IczpNet.Chat.AppVersions;

///<summary>
/// 修改 
///</summary>
[Serializable]
public class AppVersionUpdateInput
{
    /// <summary>
    /// 版本号
    /// </summary>
    [MaxLength(50)]
    [Required]
    public virtual string Version { get; set; }

    /// <summary>
    /// 版本编码(升级主要依据)
    /// </summary>
    [Required]
    public virtual long VersionCode { get; set; }

    /// <summary>
    /// 应用ID
    /// </summary>
    [MaxLength(50)]
    [Required]
    public virtual string AppId { get; set; }

    /// <summary>
    /// 标题（更新日志）
    /// </summary>
    [MaxLength(200)]
    [Required]
    public virtual string Title { get; set; }

    /// <summary>
    /// 内容（更新日志）
    /// </summary>
    [MaxLength(5000)]
    public virtual string Content { get; set; }

    /// <summary>
    /// 版本特征（更新完后显示）
    /// </summary>
    [MaxLength(500)]
    public virtual string Features { get; set; }

    /// <summary>
    /// 下载页面(Web)
    /// </summary>
    [MaxLength(500)]
    public virtual string PageUrl { get; set; }

    /// <summary>
    /// 包下载址
    /// </summary>
    [StringLength(500)]
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

    /// <summary>
    /// 
    /// </summary>
    public virtual List<Guid> DeviceGroupIdList { get; set; } = [];
}