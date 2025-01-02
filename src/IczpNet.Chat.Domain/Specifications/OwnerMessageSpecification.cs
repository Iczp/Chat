using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications;

/// <summary>
/// 我的消息(所有)
/// </summary>
public class OwnerMessageSpecification : Specification<Message>
{
    public virtual long OwnerId { get; }
    public OwnerMessageSpecification(long ownerId)
    {
        OwnerId = ownerId;
    }

    public override Expression<Func<Message, bool>> ToExpression()
    {
        return x => x.Session.UnitList.Any(d => d.OwnerId == OwnerId && (!d.Setting.HistoryFristTime.HasValue|| d.Setting.HistoryFristTime <= x.CreationTime));
    }
}
