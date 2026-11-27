using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.MessageReports.Dtos;

public class MessageReportAdminGetListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public MessageReportTypes ReportType { get; set; } = MessageReportTypes.Day;

    public Guid? SessionId { get; set; }

    public List<MessageTypes> MessageTypes { get; set; }

    public long? DateBucket { get; set; }

    public long? StartDateBucket { get; set; }

    public long? EndDateBucket { get; set; }
}

public class MessageReportAdminDto
{
    public Guid SessionId { get; set; }

    public MessageTypes MessageType { get; set; }

    public long DateBucket { get; set; }

    public long Count { get; set; }
}

public class MessageReportAdminSummaryDto
{
    public Guid SessionId { get; set; }

    public long DateBucket { get; set; }

    public long TotalCount { get; set; }
}

public class MessageReportAdminOptionsDto
{
    public bool Enable { get; set; }

    public int FlushToDbTimerPeriodSeconds { get; set; }

    public bool UseDistributedLock { get; set; }
}
