using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    /// <summary>
    /// 会话单元的消息
    /// </summary>
    public class SessionUnitMessageSpecification : Specification<Message>
    {
        public virtual SessionUnit SessionUnit { get; }
        public SessionUnitMessageSpecification(SessionUnit sessionUnit)
        {
            SessionUnit = sessionUnit;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x =>
                //!x.IsRollbacked &&
                x.AutoId > SessionUnit.ReadedMessageAutoId &&
                x.SenderId != SessionUnit.OwnerId &&
                (!SessionUnit.HistoryFristTime.HasValue || x.CreationTime > SessionUnit.HistoryFristTime)
            ;
        }
    }
}
