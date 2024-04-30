using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.BaseEntities;

public abstract class BaseSessionEntity<TKey> : BaseEntity<Guid>, IChatOwner<long>, IChatDestination<long?>
{
    /// <summary>
    /// 所属聊天对象
    /// </summary>
    public virtual long OwnerId { get; protected set; }

    /// <summary>
    /// 所属聊天对象
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 目标聊天对象
    /// </summary>
    public virtual long? DestinationId { get; protected set; }

    /// <summary>
    /// 目标聊天对象
    /// </summary>
    [ForeignKey(nameof(DestinationId))]
    public virtual ChatObject Destination { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    protected BaseSessionEntity() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    protected BaseSessionEntity(Guid id) : base(id) { }
}
