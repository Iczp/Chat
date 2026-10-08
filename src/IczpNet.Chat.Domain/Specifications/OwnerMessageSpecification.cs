using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;
using IczpNet.Chat.SessionUnitSettings;

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

    public static bool IsVisible(SessionMessageWindowCacheItem.Entry message, Guid sessionUnitId, SessionUnitSettingCacheItem setting)
        => setting.IsKilled == false &&
            (!message.IsPrivate || message.SenderSessionUnitId == sessionUnitId || message.ReceiverSessionUnitId == sessionUnitId) &&
            (!setting.StartMessageId.HasValue || message.Id > setting.StartMessageId.Value) &&
            (!setting.EndMessageId.HasValue || message.Id <= setting.EndMessageId.Value) &&
            (!setting.HistoryFristTime.HasValue || message.CreationTime >= setting.HistoryFristTime.Value) &&
            (!setting.HistoryLastTime.HasValue || message.CreationTime < setting.HistoryLastTime.Value) &&
            (!setting.ClearTime.HasValue || message.CreationTime > setting.ClearTime.Value);
}
