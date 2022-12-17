using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    /// <summary>
    /// 我的未消息
    /// </summary>
    public class UnreadedMessageSpecification : Specification<Message>
    {
        public virtual Guid OwnerId { get; }
        public UnreadedMessageSpecification(Guid ownerId)
        {
            OwnerId = ownerId;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x => x.Session.UnitList.Any(d => d.OwnerId == OwnerId && d.HistoryFristTime <= x.CreationTime && d.ReadedMessageAutoId < x.AutoId && x.SenderId != d.OwnerId);
        }
    }
}
