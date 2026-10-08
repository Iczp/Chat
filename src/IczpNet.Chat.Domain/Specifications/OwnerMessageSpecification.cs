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
    public Guid? SessionUnitId { get; }
    public OwnerMessageSpecification(long ownerId, Guid? sessionUnitId = null)
    {
        OwnerId = ownerId;
        SessionUnitId = sessionUnitId;
    }

    public override Expression<Func<Message, bool>> ToExpression()
    {
        return x => x.Session.UnitList.Any(d =>
            d.OwnerId == OwnerId && (!SessionUnitId.HasValue || d.Id == SessionUnitId) &&
            !d.Setting.IsKilled &&
            (!x.IsPrivate || x.SenderSessionUnitId == d.Id || x.ReceiverSessionUnitId == d.Id) &&
            (!d.Setting.StartMessageId.HasValue || x.Id > d.Setting.StartMessageId) &&
            (!d.Setting.EndMessageId.HasValue || x.Id <= d.Setting.EndMessageId) &&
            (!d.Setting.HistoryFristTime.HasValue || x.CreationTime >= d.Setting.HistoryFristTime) &&
            (!d.Setting.HistoryLastTime.HasValue || x.CreationTime < d.Setting.HistoryLastTime) &&
            (!d.Setting.ClearTime.HasValue || x.CreationTime > d.Setting.ClearTime) &&
            !x.DeletedList.Any(r => r.SessionUnitId == d.Id));
    }
}
