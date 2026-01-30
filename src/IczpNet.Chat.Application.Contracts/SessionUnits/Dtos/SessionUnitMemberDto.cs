using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionTags;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDto : SessionUnitInfoBase //SessionUnitSenderInfo //: SessionUnitCacheItem SessionUnitInfoBase
{
    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Owner { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual List<SessionTagCacheItem> TagList { get; set; }

    ///// <summary>
    ///// 最后发送的消息
    ///// </summary>
    //public virtual MessageSimpleDto LastSendMessage { get; set; }
}
