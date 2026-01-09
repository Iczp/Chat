using System;

namespace IczpNet.Chat.MessageReports;

public class MessageSummaryDto
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid SessionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long DateBucket { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long TotalCount { get; set; }
}