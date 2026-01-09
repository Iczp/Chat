using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Volo.Abp.DistributedLocking;

namespace IczpNet.Chat.MessageReports;

public class MessageReportManager(
    IAbpDistributedLock abpDistributedLock,
    IDateBucket dateBucket,
    IOptions<MessageReportOptions> reportOptions,
    IMessageReportDayRepository messageReportDayRepository,
    IMessageReportMonthRepository messageReportMonthRepository,
    IMessageReportHourRepository messageReportHourRepository) : RedisService, IMessageReportManager
{
    public IAbpDistributedLock AbpDistributedLock { get; } = abpDistributedLock;
    public IDateBucket DateBucket { get; } = dateBucket;
    public IOptions<MessageReportOptions> ReportOptions { get; } = reportOptions;
    public IMessageReportDayRepository MessageReportDayRepository { get; } = messageReportDayRepository;
    public IMessageReportMonthRepository MessageReportMonthRepository { get; } = messageReportMonthRepository;
    public IMessageReportHourRepository MessageReportHourRepository { get; } = messageReportHourRepository;

    protected string Prefix => $"{Options.Value.KeyPrefix}MessageReports:";

    private readonly TimeSpan? _cacheExpire = TimeSpan.FromDays(7);

    protected virtual async Task StatByDbAsync(Message message)
    {
        var sessionId = message.SessionId.Value;
        var messageType = message.MessageType;
        await MessageReportMonthRepository.IncrementAsync(sessionId, messageType, "yyyyMM");
        await MessageReportDayRepository.IncrementAsync(sessionId, messageType, "yyyyMMdd");
        await MessageReportHourRepository.IncrementAsync(sessionId, messageType, "yyyyMMddHH");
    }

    protected long BuildDateBucket(MessageReportTypes reportType)
    {
        var format = reportType switch
        {
            MessageReportTypes.Day => "yyyyMMdd",
            MessageReportTypes.Hour => "yyyyMMddHH",
            MessageReportTypes.Month => "yyyyMM",
            _ => throw new NotImplementedException()
        };
        return DateBucket.Create(Clock.Now, format);
    }

    protected string BuildKey(MessageReportTypes reportType, long dateBucket)
    {
        return $"{Prefix}{reportType}:{dateBucket}";
    }
    protected string BuildKey(MessageReportTypes reportType)
    {
        var dateBucket = BuildDateBucket(reportType);
        return $"{Prefix}{reportType}:{dateBucket}";
    }

    protected virtual Task IncrementOneAsync(MessageReportTypes reportType, Message message)
    {
        var key = BuildKey(reportType);
        //SessionId:MessageType
        var field = $"{message.SessionId}:{message.MessageType}";
        return Database.HashIncrementAsync(key, field, 1);
    }

    public async Task InitializationAsync()
    {
        //创建Sql数据结构
        await MessageReportDayRepository.EnsureMessageReportMergeTypeAsync();

        //补全统计
        await CompensateAsync();
    }

    public async Task StatAsync(Message message)
    {
        await IncrementOneAsync(MessageReportTypes.Day, message);
        await IncrementOneAsync(MessageReportTypes.Hour, message);
        await IncrementOneAsync(MessageReportTypes.Month, message);
    }

    public async Task<bool> FlushAsync(MessageReportTypes reportType, long? dateBucketInput = null)
    {
        var dateBucket = dateBucketInput ?? BuildDateBucket(reportType);

        var sourceKey = BuildKey(reportType, dateBucket);

        var isExists = await Database.KeyExistsAsync(sourceKey);

        Logger.LogInformation("FlushAsync {reportType}, DataBucket:{dateBucket}, isExists:{isExists}", reportType, dateBucket, isExists);

        if (!isExists)
        {
            return false;
        }

        if (ReportOptions.Value.UseDistributedLock)
        {
            //0. 分布式锁
            var lockerName = $"DistributedLock:{sourceKey}";
            await using var handle = await AbpDistributedLock.TryAcquireAsync(lockerName);
            Logger.LogInformation("Handle=={handle},LockerName={LockerName}", handle, lockerName);
            if (handle == null)
            {
                Logger.LogInformation("AbpDistributedLock Handle==null, {sourceKey} is processing", sourceKey);
                return false;
            }
        }

        // 1. 原子切换
        var processingKey = $"{sourceKey}:processing";

        await Database.KeyRenameAsync(sourceKey, processingKey);

        // 2. 读取数据
        var entries = await Database.HashGetAllAsync(processingKey);

        Logger.LogInformation("DataBucket: {dateBucket}, entries count:{Count}", dateBucket, entries.Length);

        if (entries.Length == 0)
        {
            await Database.KeyDeleteAsync(processingKey);
            return false;
        }

        // 3. 落库
        await FlushToDbAsync(reportType, dateBucket, entries);

        // 4. 删除 processing
        await Database.KeyDeleteAsync(processingKey);

        //// 4. 改为已完成 用于调试
        //var completedKey = $"{sourceKey}:completed";
        //// 原子切换
        //await Database.KeyRenameAsync(processingKey, completedKey);
        //await Database.KeyExpireAsync(completedKey, TimeSpan.FromSeconds(300));

        return true;
    }

    private static DataTable ToDataTable(long dateBucket, HashEntry[] entries)
    {
        var table = new DataTable();
        table.Columns.Add(nameof(MessageReportBase.SessionId), typeof(Guid));
        table.Columns.Add(nameof(MessageReportBase.MessageType), typeof(int));
        table.Columns.Add(nameof(MessageReportBase.DateBucket), typeof(long));
        table.Columns.Add(nameof(MessageReportBase.Count), typeof(long));

        foreach (var e in entries)
        {
            //SessionId:MessageType
            var parts = e.Name.ToString().Split(':');
            var sessionId = Guid.Parse(parts[0]);
            var messageType = Enum.Parse<MessageTypes>(parts[1]);
            //var dateBucket = long.Parse(dateBucket);
            var count = (long)e.Value;

            table.Rows.Add(sessionId, messageType, dateBucket, count);
        }
        return table;
    }

    protected async Task FlushToDbAsync(MessageReportTypes reportType, long dateBucket, HashEntry[] entries)
    {
        var stopwatch = Stopwatch.StartNew();
        IMessageReportRepository repository = reportType switch
        {
            MessageReportTypes.Day => MessageReportDayRepository,
            MessageReportTypes.Hour => MessageReportHourRepository,
            MessageReportTypes.Month => MessageReportMonthRepository,
            _ => throw new NotImplementedException()
        };

        Logger.LogInformation("FlushToDbAsync: {reportType}, entries count{Count}", reportType, entries.Length);

        var dataTable = ToDataTable(dateBucket, entries);

        await repository.FlushToDbAsync(dataTable);

        Logger.LogInformation("FlushToDbAsync: {reportType}, entries count{Count},Elapsed:{ms}ms", reportType, entries.Length, stopwatch.ElapsedMilliseconds);
    }

    public Task<bool> FlushMonthAsync()
    {
        return FlushAsync(MessageReportTypes.Month);
    }

    public Task<bool> FlushDayAsync()
    {
        return FlushAsync(MessageReportTypes.Day);
    }

    public Task<bool> FlushHourAsync()
    {
        return FlushAsync(MessageReportTypes.Hour);
    }

    /// <summary>
    /// 在启动时补全
    /// </summary>
    /// <returns></returns>
    public async Task CompensateAsync()
    {
        Logger.LogInformation("CompensateAsync");

        var now = Clock.Now;

        // hour（一天 24 个）
        for (int h = 0; h < 72; h++)
        {
            var hour = now.AddHours(-h);
            await FlushAsync(MessageReportTypes.Hour, DateBucket.Create(hour, MessageReportTypes.Hour));
        }

        for (int i = 0; i <= 3; i++)
        {
            var day = now.Date.AddDays(-i);
            // day
            await FlushAsync(MessageReportTypes.Day, DateBucket.Create(day, MessageReportTypes.Day));
        }

        // month（一般只需要补 1～2 个月）
        for (int i = 0; i <= 2; i++)
        {
            var month = now.AddMonths(-i);
            await FlushAsync(MessageReportTypes.Month, DateBucket.Create(month, MessageReportTypes.Month));
        }
    }
}
