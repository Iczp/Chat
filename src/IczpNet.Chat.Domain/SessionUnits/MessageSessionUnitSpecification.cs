﻿using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.SessionUnits
{
    /// <summary>
    /// 会话单元的消息
    /// </summary>
    public class MessageSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual Message Message { get; }
        public MessageSessionUnitSpecification(Message message)
        {
            Message = message;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            return x =>
                (!x.Setting.HistoryFristTime.HasValue || Message.CreationTime > x.Setting.HistoryFristTime) &&
                (!x.Setting.HistoryLastTime.HasValue || Message.CreationTime < x.Setting.HistoryLastTime)
            ;
        }
    }
}
