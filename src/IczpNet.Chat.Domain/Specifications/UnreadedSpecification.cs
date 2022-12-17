using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    /// <summary>
    /// 未读消息
    /// </summary>
    public class UnreadedSpecification : Specification<Message>
    {
        public virtual SessionUnit SessionUnit { get; }
        public UnreadedSpecification(SessionUnit sessionUnit)
        {
            SessionUnit = sessionUnit;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x => x.AutoId > SessionUnit.ReadedMessageAutoId && x.SenderId != SessionUnit.OwnerId && x.CreationTime > SessionUnit.HistoryFristTime;
        }
    }
}
