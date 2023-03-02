using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    public class MaxAutoIdMessageSpecification : Specification<Message>
    {
        public virtual long MaxAutoId { get; }
        public MaxAutoIdMessageSpecification(long maxAutoId)
        {
            MaxAutoId = maxAutoId;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x => x.Id <= MaxAutoId;
        }
    }
}
