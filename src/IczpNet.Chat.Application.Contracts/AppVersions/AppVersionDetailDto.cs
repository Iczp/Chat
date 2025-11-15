using System;
namespace IczpNet.Chat.AppVersions;
///<summary>
/// 详情 
///</summary>
[Serializable]
public class AppVersionDetailDto : AppVersionDto
{
    /// <summary>
    /// 内容（更新日志）
    /// </summary>
    public virtual string Content { get; set; }

    /// <summary>
    /// 版本特征（更新完后显示）
    /// </summary>
    public virtual string Features { get; set; }
}