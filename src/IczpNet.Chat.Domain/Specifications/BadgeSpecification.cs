using Castle.Core.Resource;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.Specifications
{
    /// <summary>
    /// 未读消息
    /// </summary>
    internal class UnreadedSpecification : Specification<Message>
    {

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return x => x.Session.UnitList.Any(m => m.OwnerId != x.SenderId && m.HistoryFristTime <= x.CreationTime && m.ReadedMessageAutoId < x.AutoId && x.SenderId != m.OwnerId);
        }
    }
}
