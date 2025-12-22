using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageOwnerDto : MessageQuoteDto, IEntityDto<long>
{
    ///// <summary>
    ///// 朋友Id
    ///// </summary>
    //public virtual Guid? FriendshipSessionUnitId { get; set; }

    /// <summary>
    /// 引用消息
    /// </summary>
    public virtual MessageQuoteDto QuoteMessage { get; set; }

    /// <summary>
    /// 转发来源Id(转发才有)
    /// </summary>
    public virtual long? ForwardMessageId { get; set; }

    /// <summary>
    /// 转发层级 0:不是转发
    /// </summary>
    public virtual long ForwardDepth { get; set; }

    /// <summary>
    /// 引用来源Id(引用才有)
    /// </summary>
    public virtual long? QuoteMessageId { get; set; }

    /// <summary>
    /// 引用层级 0:不是引用
    /// </summary>
    public virtual long QuoteDepth { get; set; }

    /// <summary>
    /// 是否@所有人
    /// </summary>
    public virtual bool IsRemindAll { get; set; }

    /// <summary>
    /// 是否@我
    /// </summary>
    public virtual bool? IsRemindMe { get; set; }

    /// <summary>
    /// 是否已打开
    /// </summary>
    public virtual bool? IsOpened { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public virtual bool? IsReaded { get; set; }

    /// <summary>
    /// 是否特别关注
    /// </summary>
    public virtual bool? IsFollowing { get; set; }

    /// <summary>
    /// 是否收藏
    /// </summary>
    public virtual bool? IsFavorited { get; set; }

    /// <summary>
    /// 成员数量
    /// </summary>
    public virtual long? SessionUnitCount { get; set; }

    /// <summary>
    /// 已读数量
    /// </summary>
    public virtual long? ReadedCount { get; set; }

    /// <summary>
    /// 打开数量
    /// </summary>
    public virtual long? OpenedCount { get; set; }

    /// <summary>
    /// 收藏数量
    /// </summary>
    public virtual long? FavoritedCount { get; set; }
}
