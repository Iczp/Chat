using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    /// <summary>
    /// 发送人的消息
    /// </summary>
    public class SenderMessageSpecification : Specification<Message>
    {
        public virtual Guid SenderId { get; }
        public SenderMessageSpecification(Guid senderId)
        {
            SenderId = senderId;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x => x.SenderId == SenderId;
        }
    }
}
