using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    /// <summary>
    /// 我的消息(单个会话)
    /// </summary>
    public class SessionMessageSpecification : Specification<Message>
    {
        public virtual Guid SessionId { get; }
        public SessionMessageSpecification(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x => x.SessionId == SessionId;
        }
    }
}
