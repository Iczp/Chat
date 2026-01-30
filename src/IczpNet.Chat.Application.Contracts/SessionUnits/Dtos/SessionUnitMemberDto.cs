using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionTags;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDto : SessionUnitInfoBase //SessionUnitSenderInfo //: SessionUnitCacheItem SessionUnitInfoBase
{
    /// <summary>
    /// 最后发送时间
    /// </summary>
    public DateTime? LastSendTime { get; set; }

    /// <summary>
    /// 最后发送消息Id
    /// </summary>
    public virtual long? LastSendMessageId { get; set; }

    ///// <summary>
    ///// 最后发送的消息
    ///// </summary>
    //public virtual MessageSimpleDto LastSendMessage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Owner { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual List<SessionTagCacheItem> TagList { get; set; }

}
