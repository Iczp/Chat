using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
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
                (SessionUnit.Setting.ReadedMessageId == null || x.Id > SessionUnit.Setting.ReadedMessageId) &&
                x.SenderId != SessionUnit.OwnerId &&
                (!SessionUnit.Setting.HistoryFristTime.HasValue || x.CreationTime > SessionUnit.Setting.HistoryFristTime) &&
                (!SessionUnit.Setting.HistoryLastTime.HasValue || x.CreationTime < SessionUnit.Setting.HistoryLastTime) &&
                (!SessionUnit.Setting.ClearTime.HasValue || x.CreationTime > SessionUnit.Setting.ClearTime)

            ;
        }
    }
}
