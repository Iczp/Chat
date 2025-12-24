using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageReports;

public class MessageReportDto 
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid SessionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual MessageTypes MessageType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long DateBucket { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long Count { get; set; }
}