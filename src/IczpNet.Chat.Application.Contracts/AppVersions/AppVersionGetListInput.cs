using IczpNet.Chat.BaseDtos;
using System;
namespace IczpNet.Chat.AppVersions;

///<summary>
/// 查询列表 
///</summary>
[Serializable]
public class AppVersionGetListInput : GetListInput
{
    /// <summary>
    /// 版本编码(升级主要依据)
    /// </summary>
    public virtual long? VersionCode { get; set; }

    /// <summary>
    /// 版本号起始
    /// </summary>
    public virtual long? VersionCodeStart { get; set; }

    /// <summary>
    /// 版本号终止
    /// </summary>
    public virtual long? VersionCodeEnd { get; set; }

    /// <summary>
    /// 应用ID
    /// </summary>
    public virtual string AppId { get; set; }

    /// <summary>
    /// 适用平台 ios | android | electron
    /// </summary>
    public virtual string Platform { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public virtual bool? IsForce { get; set; }

    /// <summary>
    /// 是否热更新包(.wgt)
    /// </summary>
    public virtual bool? IsWidget { get; set; }

    /// <summary>
    /// 公开，则全部可以使用，否则只有指定的分组可以使用
    /// </summary>
    public virtual bool? IsPublic { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool? IsEnabled { get; set; }
}