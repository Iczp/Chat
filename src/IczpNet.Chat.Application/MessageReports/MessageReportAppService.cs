using IczpNet.Chat.BaseAppServices;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageReports;

public class MessageReportAppService(
    IMessageReportDayRepository messageReportDayRepository,
    IMessageReportMonthRepository messageReportMonthRepository,
    IMessageReportHourRepository messageReportHourRepository,
    IOptions<MessageReportOptions> options) : ChatAppService, IMessageReportAppService
{
    public IMessageReportDayRepository MessageReportDayRepository { get; } = messageReportDayRepository;
    public IMessageReportMonthRepository MessageReportMonthRepository { get; } = messageReportMonthRepository;
    public IMessageReportHourRepository MessageReportHourRepository { get; } = messageReportHourRepository;
    public IOptions<MessageReportOptions> Options { get; } = options;

    protected IMessageReportManager MessageReportManager => LazyServiceProvider.LazyGetRequiredService<IMessageReportManager>();

    /// <summary>
    /// 统计落库
    /// </summary>
    /// <param name="granularity">Month/Day/Hour</param>
    /// <param name="dateBucket"></param>
    /// <returns></returns>
    public Task<bool> FlushAsync(string granularity, long dateBucket)
    {
        return MessageReportManager.FlushAsync(granularity, dateBucket);
    }

    /// <summary>
    /// 配置信息
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public Task<MessageReportOptions> GetOptionsAsync()
    {
        return Task.FromResult(Options.Value);
    }

    protected async Task<IQueryable<MessageReportBase>> CreateQueryableAsync(string granularity, MessageReportGetListInput input)
    {

        IQueryable<MessageReportBase> queryable = granularity switch
        {
            "Day" => await MessageReportDayRepository.GetQueryableAsync(),
            "Hour" => await MessageReportHourRepository.GetQueryableAsync(),
            "Month" => await MessageReportMonthRepository.GetQueryableAsync(),
            _ => throw new NotImplementedException()
        };

        var query = queryable
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.MessageType.HasValue, x => x.MessageType == input.MessageType)
            .WhereIf(input.DateBucket.HasValue, x => x.DateBucket == input.DateBucket)
            .WhereIf(input.StartDateBucket.HasValue, x => x.DateBucket >= input.StartDateBucket)
            .WhereIf(input.EndDateBucket.HasValue, x => x.DateBucket < input.EndDateBucket)
            ;

        return query;
    }

    protected async Task<PagedResultDto<MessageReportDto>> GetListInternalAsync(string granularity, MessageReportGetListInput input)
    {
        var query = await CreateQueryableAsync(granularity, input);

        return await GetPagedListAsync<MessageReportBase, MessageReportDto>(query, input, x => x.OrderByDescending(x => x.DateBucket));
    }

    /// <summary>
    /// 报表
    /// </summary>
    /// <param name="granularity">Month/Day/Hour</param>
    /// <param name="input"></param>
    /// <returns></returns>
    public Task<PagedResultDto<MessageReportDto>> GetListAsync(string granularity, MessageReportGetListInput input)
    {
        return granularity?.ToLower() switch
        {
            "day" => GetListInternalAsync("Day", input),
            "hour" => GetListInternalAsync("Hour", input),
            "month" => GetListInternalAsync("Month", input),
            _ => throw new NotImplementedException()
        };
    }
}
