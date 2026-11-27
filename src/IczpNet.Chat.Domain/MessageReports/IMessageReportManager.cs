using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageReports;

public class MessageReportAdminItemModel
{
    public Guid SessionId { get; set; }

    public MessageTypes MessageType { get; set; }

    public long DateBucket { get; set; }

    public long Count { get; set; }
}

public class MessageReportAdminListResult
{
    public long TotalCount { get; set; }

    public List<MessageReportAdminItemModel> Items { get; set; } = new();
}

public class MessageReportAdminSummaryItemModel
{
    public Guid SessionId { get; set; }

    public long DateBucket { get; set; }

    public long TotalCount { get; set; }
}

public class MessageReportAdminSummaryResult
{
    public long TotalCount { get; set; }

    public List<MessageReportAdminSummaryItemModel> Items { get; set; } = new();
}

public interface IMessageReportManager
{
    /// <summary>
    /// 创建数据结构
    /// </summary>
    /// <returns></returns>
    Task InitializationAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task StatAsync(MessageCacheItem message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reportType">Month | Day | Hour</param>
    /// <param name="dateBucket"></param>
    /// <returns></returns>
    Task<bool> FlushAsync(MessageReportTypes reportType, long? dateBucket = null);

    Task<bool> FlushMonthAsync();

    Task<bool> FlushDayAsync();

    Task<bool> FlushHourAsync();

    /// <summary>
    /// 补偿
    /// </summary>
    /// <returns></returns>
    Task CompensateAsync();
}

public interface IMessageReportAdminManager : IMessageReportManager
{
    MessageReportOptions GetAdminOptions();

    Task<MessageReportAdminListResult> GetAdminListAsync(
        MessageReportTypes reportType,
        Guid? sessionId = null,
        List<MessageTypes> messageTypes = null,
        long? dateBucket = null,
        long? startDateBucket = null,
        long? endDateBucket = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<MessageReportAdminSummaryResult> GetAdminSummaryAsync(
        MessageReportTypes reportType,
        Guid? sessionId = null,
        List<MessageTypes> messageTypes = null,
        long? dateBucket = null,
        long? startDateBucket = null,
        long? endDateBucket = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);
}
