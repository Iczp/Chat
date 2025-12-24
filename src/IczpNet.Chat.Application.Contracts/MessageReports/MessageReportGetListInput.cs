using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageReports;

public class MessageReportGetListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual MessageTypes? MessageType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? DateBucket { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? StartDateBucket { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? EndDateBucket { get; set; }
}