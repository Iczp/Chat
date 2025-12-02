using IczpNet.AbpCommons.Dtos;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitCacheItemGetListInput : GetListInput
{
    /// <summary>
    /// 聊天对象所属者Id
    /// </summary>
    [Required]
    public long OwnerId { get; set; }

    /// <summary>
    /// 是否有提醒 @我/@所有人
    /// </summary>
    public virtual bool? IsRemind { get; set; }

    /// <summary>
    /// 是否有角标（新消息）
    /// </summary>
    public virtual bool? IsBadge { get; set; }

    ///// <summary>
    ///// 是否我正在关注该聊天对象
    ///// </summary>
    //public virtual bool? IsFollowing { get; set; }

    ///// <summary>
    ///// 是否关注我的
    ///// </summary>
    //public virtual bool? IsFollower { get; set; }

    /// <summary>
    /// 目标聊天对象Id
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// 聊天对象类型:个人|群|服务号等
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

    /// <summary>
    /// 最小消息Id
    /// </summary>
    public virtual long? MinMessageId { get; set; }

    /// <summary>
    /// 最大消息Id
    /// </summary>
    public virtual long? MaxMessageId { get; set; }

    /// <summary>
    /// 最小时间戳
    /// </summary>
    public virtual long? MinTicks { get; set; }

    /// <summary>
    /// 最大时间戳
    /// </summary>
    public virtual long? MaxTicks { get; set; }


}
