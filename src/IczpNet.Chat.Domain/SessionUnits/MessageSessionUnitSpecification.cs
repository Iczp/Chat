using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话单元的消息
/// </summary>
public class MessageSessionUnitSpecification(Message message) : Specification<SessionUnit>
{
    public virtual Message Message { get; } = message;

    public override Expression<Func<SessionUnit, bool>> ToExpression()
    {
        return x =>
            (!x.Setting.HistoryFristTime.HasValue || Message.CreationTime > x.Setting.HistoryFristTime) &&
            (!x.Setting.HistoryLastTime.HasValue || Message.CreationTime < x.Setting.HistoryLastTime)
        ;
    }
}
