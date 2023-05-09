using IczpNet.Chat.Enums;
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
            //d.SenderId != x.OwnerId &&
            //    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
            //    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
            //    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
            //    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime))
            return x =>
                //!x.IsRollbacked &&
                (SessionUnit.ReadedMessageId == null || x.Id > SessionUnit.ReadedMessageId) &&
                SessionUnit.ServiceStatus == ServiceStatus.Normal &&
                x.SenderId != SessionUnit.OwnerId &&
                (!SessionUnit.HistoryFristTime.HasValue || x.CreationTime > SessionUnit.HistoryFristTime) &&
                (!SessionUnit.HistoryLastTime.HasValue || x.CreationTime < SessionUnit.HistoryLastTime) &&
                (!SessionUnit.ClearTime.HasValue || x.CreationTime > SessionUnit.ClearTime)

            ;
        }
    }
}
