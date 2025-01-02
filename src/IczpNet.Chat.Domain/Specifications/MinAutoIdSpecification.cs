using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications;

public class MinAutoIdMessageSpecification : Specification<Message>
{
    public virtual long MinAutoId { get; }
    public MinAutoIdMessageSpecification(long minAutoId)
    {
        MinAutoId = minAutoId;
    }

    public override Expression<Func<Message, bool>> ToExpression()
    {
        return x => x.Id > MinAutoId;
    }
}
