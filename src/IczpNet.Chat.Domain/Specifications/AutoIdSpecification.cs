using IczpNet.Chat.MessageSections.Messages;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    public class AutoIdMessageSpecification : AndSpecification<Message>
    {
        public virtual long MinAutoId { get; }
        public virtual long MaxAutoId { get; }
        public AutoIdMessageSpecification(long minAutoId, long maxAutoId) : base(new MinAutoIdMessageSpecification(minAutoId), new MaxAutoIdMessageSpecification(maxAutoId))
        {
            MinAutoId = minAutoId;
            MaxAutoId = maxAutoId;
        }

        //public override Expression<Func<Message, bool>> ToExpression()
        //{
        //    return x => x.AutoId > MinAutoId && x.AutoId <= MaxAutoId; 
        //}
    }
}
