using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberGetListInput : GetListInput
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

    /// <summary>
    /// 是否创建者（群主）
    /// </summary>
    public bool? IsCreator { get; set; }

    /// <summary>
    /// 所属成员Id
    /// </summary>
    public long? OwnerId { get; set; }
}
