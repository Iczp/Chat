using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageReports;

public class MessageReportSummaryGetListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 消息类型集合
    /// </summary>
    public virtual List<MessageTypes> MessageTypes { get; set; }

    /// <summary>
    /// 日期桶
    /// </summary>
    public virtual long? DateBucket { get; set; }

    /// <summary>
    /// 起始日期桶
    /// </summary>
    public virtual long? StartDateBucket { get; set; }

    /// <summary>
    /// 结束日期桶
    /// </summary>
    public virtual long? EndDateBucket { get; set; }
}