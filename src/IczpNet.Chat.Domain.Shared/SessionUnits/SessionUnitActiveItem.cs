using System;

namespace IczpNet.Chat.SessionUnits;

[Serializable]
public class SessionUnitActiveItem
{
    public virtual Guid Id { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// Badge
    /// </summary>
    public virtual long? Badge { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? LastMessageId { get; set; }

}
