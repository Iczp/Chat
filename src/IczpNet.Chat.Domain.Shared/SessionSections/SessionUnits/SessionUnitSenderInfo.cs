using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitSenderInfo : SessionUnitInfoBase
{
    ///// <summary>
    ///// 
    ///// </summary>
    //public virtual string DisplayName { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Owner { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual List<SessionTagInfo> TagList { get; set; }
}
