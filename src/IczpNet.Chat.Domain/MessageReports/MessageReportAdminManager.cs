using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageReports;

public class MessageReportAdminManager(
    IMessageReportManager messageReportManager,
    IOptions<MessageReportOptions> reportOptions,
    IMessageReportDayRepository messageReportDayRepository,
    IMessageReportMonthRepository messageReportMonthRepository,
    IMessageReportHourRepository messageReportHourRepository) : DomainService, IMessageReportAdminManager
{
    protected IMessageReportManager InnerManager { get; } = messageReportManager;
    protected IOptions<MessageReportOptions> ReportOptions { get; } = reportOptions;
    protected IMessageReportDayRepository MessageReportDayRepository { get; } = messageReportDayRepository;
    protected IMessageReportMonthRepository MessageReportMonthRepository { get; } = messageReportMonthRepository;
    protected IMessageReportHourRepository MessageReportHourRepository { get; } = messageReportHourRepository;

    public Task InitializationAsync() => InnerManager.InitializationAsync();
    public Task StatAsync(MessageCacheItem message) => InnerManager.StatAsync(message);
    public Task<bool> FlushAsync(MessageReportTypes reportType, long? dateBucket = null) => InnerManager.FlushAsync(reportType, dateBucket);
    public Task<bool> FlushMonthAsync() => InnerManager.FlushMonthAsync();
    public Task<bool> FlushDayAsync() => InnerManager.FlushDayAsync();
    public Task<bool> FlushHourAsync() => InnerManager.FlushHourAsync();
    public Task CompensateAsync() => InnerManager.CompensateAsync();

    public virtual MessageReportOptions GetAdminOptions()
    {
        return ReportOptions.Value;
    }

    public virtual async Task<MessageReportAdminListResult> GetAdminListAsync(
        MessageReportTypes reportType,
        Guid? sessionId = null,
        List<MessageTypes> messageTypes = null,
        long? dateBucket = null,
        long? startDateBucket = null,
        long? endDateBucket = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = await CreateReportQueryableAsync(reportType);
        var query = ApplyReportFilters(queryable, sessionId, messageTypes, dateBucket, startDateBucket, endDateBucket);

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new MessageReportAdminListResult();
        }

        query = ApplyReportSorting(query, sorting);

        var items = await AsyncExecuter.ToListAsync(
            query
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200))
                .Select(x => new MessageReportAdminItemModel
                {
                    SessionId = x.SessionId,
                    MessageType = x.MessageType,
                    DateBucket = x.DateBucket,
                    Count = x.Count
                }),
            cancellationToken);

        return new MessageReportAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<MessageReportAdminSummaryResult> GetAdminSummaryAsync(
        MessageReportTypes reportType,
        Guid? sessionId = null,
        List<MessageTypes> messageTypes = null,
        long? dateBucket = null,
        long? startDateBucket = null,
        long? endDateBucket = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = await CreateReportQueryableAsync(reportType);
        var filtered = ApplyReportFilters(queryable, sessionId, messageTypes, dateBucket, startDateBucket, endDateBucket);

        var summaryQuery = filtered
            .GroupBy(x => x.DateBucket)
            .Select(g => new MessageReportAdminSummaryItemModel
            {
                SessionId = g.Select(x => x.SessionId).FirstOrDefault(),
                DateBucket = g.Key,
                TotalCount = g.Sum(x => x.Count)
            });

        var totalCount = await AsyncExecuter.LongCountAsync(summaryQuery, cancellationToken);
        if (totalCount == 0)
        {
            return new MessageReportAdminSummaryResult();
        }

        var desc = string.IsNullOrWhiteSpace(sorting) || !sorting.Trim().EndsWith("asc", StringComparison.OrdinalIgnoreCase);
        summaryQuery = desc
            ? summaryQuery.OrderByDescending(x => x.DateBucket)
            : summaryQuery.OrderBy(x => x.DateBucket);

        var items = await AsyncExecuter.ToListAsync(
            summaryQuery
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new MessageReportAdminSummaryResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    private async Task<IQueryable<MessageReportBase>> CreateReportQueryableAsync(MessageReportTypes granularity)
    {
        return granularity switch
        {
            MessageReportTypes.Day => await MessageReportDayRepository.GetQueryableAsync(),
            MessageReportTypes.Hour => await MessageReportHourRepository.GetQueryableAsync(),
            MessageReportTypes.Month => await MessageReportMonthRepository.GetQueryableAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, "Unsupported report granularity.")
        };
    }

    private static IQueryable<MessageReportBase> ApplyReportFilters(
        IQueryable<MessageReportBase> queryable,
        Guid? sessionId,
        List<MessageTypes> messageTypes,
        long? dateBucket,
        long? startDateBucket,
        long? endDateBucket)
    {
        if (sessionId.HasValue)
        {
            queryable = queryable.Where(x => x.SessionId == sessionId.Value);
        }
        if (messageTypes != null && messageTypes.Count > 0)
        {
            queryable = queryable.Where(x => messageTypes.Contains(x.MessageType));
        }
        if (dateBucket.HasValue)
        {
            queryable = queryable.Where(x => x.DateBucket == dateBucket.Value);
        }
        if (startDateBucket.HasValue)
        {
            queryable = queryable.Where(x => x.DateBucket >= startDateBucket.Value);
        }
        if (endDateBucket.HasValue)
        {
            queryable = queryable.Where(x => x.DateBucket < endDateBucket.Value);
        }
        return queryable;
    }

    private static IQueryable<MessageReportBase> ApplyReportSorting(IQueryable<MessageReportBase> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.DateBucket).ThenByDescending(x => x.Count);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "count" => desc ? queryable.OrderByDescending(x => x.Count) : queryable.OrderBy(x => x.Count),
            "messagetype" => desc ? queryable.OrderByDescending(x => x.MessageType) : queryable.OrderBy(x => x.MessageType),
            "datebucket" => desc ? queryable.OrderByDescending(x => x.DateBucket) : queryable.OrderBy(x => x.DateBucket),
            _ => queryable.OrderByDescending(x => x.DateBucket)
        };
    }
}
