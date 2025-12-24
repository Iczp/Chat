using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisServices;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageReports;

public class MessageReportManager(
    IDateBucket dateBucket,
    IMessageReportDayRepository messageReportDayRepository,
    IMessageReportMonthRepository messageReportMonthRepository,
    IMessageReportHourRepository messageReportHourRepository) : RedisService, IMessageReportManager
{
    public IDateBucket DateBucket { get; } = dateBucket;
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

    protected long BuildDateBucket(string granularity)
    {
        var format = granularity switch
        {
            "Day" => "yyyyMMdd",
            "Hour" => "yyyyMMddHH",
            "Month" => "yyyyMM",
            _ => throw new NotImplementedException()
        };
        return DateBucket.Create(Clock.Now, format);
    }

    protected string BuildKey(string granularity, long dateBucket)
    {
        return $"{Prefix}{granularity}:{dateBucket}";
    }
    protected string BuildKey(string granularity)
    {
        var dateBucket = BuildDateBucket(granularity);
        return $"{Prefix}{granularity}:{dateBucket}";
    }

    protected virtual Task IncrementOneAsync(string granularity, Message message)
    {
        var key = BuildKey(granularity);
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
        await IncrementOneAsync("Day", message);
        await IncrementOneAsync("Hour", message);
        await IncrementOneAsync("Month", message);
    }

    public async Task<bool> FlushAsync(string granularity, long? dateBucketInput = null)
    {
        var dateBucket = dateBucketInput ?? BuildDateBucket(granularity);

        var sourceKey = BuildKey(granularity, dateBucket);

        var isExists = await Database.KeyExistsAsync(sourceKey);

        Logger.LogInformation("FlushAsync {granularity}, DataBucket:{dateBucket}, isExists:{isExists}", granularity, dateBucket, isExists);

        if (!isExists)
        {
            return false;
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
        await FlushToDbAsync(granularity, dateBucket, entries);

        // 4️⃣ 删除 processing
        await Database.KeyDeleteAsync(processingKey);

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

    protected async Task FlushToDbAsync(string granularity, long dateBucket, HashEntry[] entries)
    {
        var stopwatch = Stopwatch.StartNew();
        IMessageReportRepository repository = granularity switch
        {
            "Day" => MessageReportDayRepository,
            "Hour" => MessageReportHourRepository,
            "Month" => MessageReportMonthRepository,
            _ => throw new NotImplementedException()
        };

        Logger.LogInformation("FlushToDbAsync: {granularity}, entries count{Count}", granularity, entries.Length);

        var dataTable = ToDataTable(dateBucket, entries);

        await repository.FlushToDbAsync(dataTable);

        Logger.LogInformation("FlushToDbAsync: {granularity}, entries count{Count},Elapsed:{ms}ms", granularity, entries.Length, stopwatch.ElapsedMilliseconds);
    }

    public Task<bool> FlushMonthAsync()
    {
        return FlushAsync("Month");
    }

    public Task<bool> FlushDayAsync()
    {
        return FlushAsync("Day");
    }

    public Task<bool> FlushHourAsync()
    {
        return FlushAsync("Hour");
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
            await FlushAsync("Hour", DateBucket.Create(hour, "yyyyMMddHH"));
        }

        for (int i = 0; i <= 3; i++)
        {
            var day = now.Date.AddDays(-i);
            // day
            await FlushAsync("Day", DateBucket.Create(day, "yyyyMMdd"));
        }

        // month（一般只需要补 1～2 个月）
        for (int i = 0; i <= 2; i++)
        {
            var month = now.AddMonths(-i);
            await FlushAsync("Month", DateBucket.Create(month, "yyyyMM"));
        }
    }
}
