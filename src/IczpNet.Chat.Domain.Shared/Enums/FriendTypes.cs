using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 好友归档
/// 0: All,
/// 1: Pinned,
/// 2: Following,
/// 3: RemindAll,
/// 4: RemindMe,
/// 5: Immersed,
/// </summary>
[Description("好友归档")]
public enum FriendTypes
{
    /// <summary>  
    /// 全部 
    /// </summary>  
    [Description("全部")]
    All = 0,

    /// <summary>  
    /// 置顶    
    /// </summary>  
    [Description("置顶")]
    Pinned = 1,

    /// <summary>  
    /// 我关注的
    /// </summary>  
    [Description("关注")]
    Following = 2,

    /// <summary>  
    /// @所有人
    /// </summary>  
    [Description("@所有人")]
    RemindAll = 3,

    /// <summary>  
    /// @我
    /// </summary>  
    [Description("@我")]
    RemindMe = 4,

    /// <summary>  
    /// 静默
    /// </summary>  
    [Description("静默")]
    Immersed = 5,
}