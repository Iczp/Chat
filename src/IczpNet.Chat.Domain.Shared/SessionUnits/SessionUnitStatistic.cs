using System.ComponentModel;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitStatistic
{
    /// <summary>
    /// 
    /// </summary>
    [Description("消息")]
    public long PublicBadge { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Description("私有消息")] 
    public long PrivateBadge { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Description("@我")]
    public long RemindMe { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Description("@所有人")] 
    public long RemindAll { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Description("关注")]
    public long Following { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Description("免打扰")]
    public long Immersed { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Description("置顶")]
    public long Pinned { get; set; }
}
