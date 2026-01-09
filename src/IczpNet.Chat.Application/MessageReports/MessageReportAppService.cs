using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
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
    /// <param name="type">
    /// /// 消息报表类型
    /// 20: Month 月,
    /// 30: Day 日,
    /// 40: Hour 时,
    /// </param>
    /// <param name="dateBucket"></param>
    /// <returns></returns>
    public Task<bool> FlushAsync([Required] MessageReportTypes type, long dateBucket)
    {
        return MessageReportManager.FlushAsync(type, dateBucket);
    }

    /// <summary>
    /// 配置信息
    /// </summary>
    /// <returns></returns>
    public Task<MessageReportOptions> GetOptionsAsync()
    {
        return Task.FromResult(Options.Value);
    }

    protected async Task<IQueryable<MessageReportBase>> CreateQueryableAsync(MessageReportTypes granularity)
    {

        IQueryable<MessageReportBase> queryable = granularity switch
        {
            MessageReportTypes.Day => await MessageReportDayRepository.GetQueryableAsync(),
            MessageReportTypes.Hour => await MessageReportHourRepository.GetQueryableAsync(),
            MessageReportTypes.Month => await MessageReportMonthRepository.GetQueryableAsync(),
            _ => throw new NotImplementedException()
        };
        return queryable;
    }

    protected async Task<PagedResultDto<MessageReportDto>> GetListInternalAsync(MessageReportGetListInput input)
    {
        var queryable = await CreateQueryableAsync(input.ReportType);

        var query = queryable
           .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
           .WhereIf(input.MessageTypes.IsAny(), x => input.MessageTypes.Contains(x.MessageType))
           .WhereIf(input.DateBucket.HasValue, x => x.DateBucket == input.DateBucket)
           .WhereIf(input.StartDateBucket.HasValue, x => x.DateBucket >= input.StartDateBucket)
           .WhereIf(input.EndDateBucket.HasValue, x => x.DateBucket < input.EndDateBucket)
           ;

        return await GetPagedListAsync<MessageReportBase, MessageReportDto>(query, input, x => x.OrderByDescending(x => x.DateBucket));
    }

    /// <summary>
    /// 报表
    /// </summary>
    /// <returns></returns>
    public Task<PagedResultDto<MessageReportDto>> GetListAsync(MessageReportGetListInput input)
    {
        return GetListInternalAsync(input);
    }

    protected async Task<PagedResultDto<MessageSummaryDto>> GetSummaryInternalAsync(MessageReportSummaryGetListInput input)
    {
        var queryable = await CreateQueryableAsync(input.ReportType);

        var query = queryable
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.MessageTypes.IsAny(), x => input.MessageTypes.Contains(x.MessageType))
            .WhereIf(input.DateBucket.HasValue, x => x.DateBucket == input.DateBucket)
            .WhereIf(input.StartDateBucket.HasValue, x => x.DateBucket >= input.StartDateBucket)
            .WhereIf(input.EndDateBucket.HasValue, x => x.DateBucket < input.EndDateBucket)
            .GroupBy(x => x.DateBucket)
            .Select(x => new MessageSummaryDto()
            {
                SessionId = x.FirstOrDefault().SessionId,
                DateBucket = x.Key,
                TotalCount = x.Sum(y => y.Count),
            });
        return await GetPagedListAsync(query, input, x => x.OrderByDescending(x => x.DateBucket));
    }

    /// <summary>
    /// 汇总
    /// </summary>
    public Task<PagedResultDto<MessageSummaryDto>> GetSummaryAsync( MessageReportSummaryGetListInput input)
    {
        return GetSummaryInternalAsync( input);

    }


}
