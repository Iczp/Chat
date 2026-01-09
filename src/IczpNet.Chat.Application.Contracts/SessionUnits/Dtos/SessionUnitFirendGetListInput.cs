using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFirendGetListInput : GetListInput
{
    ///// <summary>
    ///// ChatObjectId
    ///// </summary>
    //[Required]
    //public long OwnerId { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MinScore { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MaxScore { get; set; }

    ///// <summary>
    ///// 是否关注
    ///// </summary>
    //public bool? IsFollowing { get; set; }

    ///// <summary>
    ///// 是否@我
    ///// </summary>
    //public bool? IsReminMe { get; set; }

    ///// <summary>
    ///// 是否@所有人
    ///// </summary>
    //public bool? IsReminAll { get; set; }

    ///// <summary>
    ///// 是否静默消息
    ///// </summary>
    //public bool? IsImmersed { get; set; }

    ///// <summary>
    ///// 是否置顶
    ///// </summary>
    //public bool? IsPinned { get; set; }
}
