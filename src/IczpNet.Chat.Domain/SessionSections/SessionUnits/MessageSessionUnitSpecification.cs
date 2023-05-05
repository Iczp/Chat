using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    /// <summary>
    /// 会话单元的消息
    /// </summary>
    public class MessageSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual Message Message { get; }
        public MessageSessionUnitSpecification(Message message)
        {
            Message = message;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            return x =>
                (!x.HistoryFristTime.HasValue || Message.CreationTime > x.HistoryFristTime) &&
                (!x.HistoryLastTime.HasValue || Message.CreationTime < x.HistoryLastTime)
            ;
        }
    }
}
