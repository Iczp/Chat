using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.MessageReports.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageReports;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.MessageReports;

/// <summary>
/// 消息报表统计管理服务
/// </summary>
[Authorize(ChatAdminPermissions.MessageReports.Default)]
[ExposeServices(typeof(MessageReportAppService), typeof(IMessageReportAdminAppService))]
public class MessageReportAppService(IMessageReportAdminManager messageReportManager)
    : ApplicationService, IMessageReportAdminAppService
{
    protected IMessageReportAdminManager MessageReportManager { get; } = messageReportManager;

    [Authorize(ChatAdminPermissions.MessageReports.GetList)]
    public virtual async Task<PagedResultDto<MessageReportAdminDto>> GetListAsync(
        MessageReportAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new MessageReportAdminGetListInput();

        var result = await MessageReportManager.GetAdminListAsync(
            input.ReportType,
            input.SessionId,
            input.MessageTypes,
            input.DateBucket,
            input.StartDateBucket,
            input.EndDateBucket,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<MessageReportAdminDto>(
            result.TotalCount,
            result.Items.Select(x => new MessageReportAdminDto
            {
                SessionId = x.SessionId,
                MessageType = x.MessageType,
                DateBucket = x.DateBucket,
                Count = x.Count
            }).ToList());
    }

    [Authorize(ChatAdminPermissions.MessageReports.GetSummary)]
    public virtual async Task<PagedResultDto<MessageReportAdminSummaryDto>> GetSummaryAsync(
        MessageReportAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new MessageReportAdminGetListInput();

        var result = await MessageReportManager.GetAdminSummaryAsync(
            input.ReportType,
            input.SessionId,
            input.MessageTypes,
            input.DateBucket,
            input.StartDateBucket,
            input.EndDateBucket,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<MessageReportAdminSummaryDto>(
            result.TotalCount,
            result.Items.Select(x => new MessageReportAdminSummaryDto
            {
                SessionId = x.SessionId,
                DateBucket = x.DateBucket,
                TotalCount = x.TotalCount
            }).ToList());
    }

    [Authorize(ChatAdminPermissions.MessageReports.GetOptions)]
    public virtual Task<MessageReportAdminOptionsDto> GetOptionsAsync()
    {
        var opts = MessageReportManager.GetAdminOptions();
        return Task.FromResult(new MessageReportAdminOptionsDto
        {
            Enable = opts.Enable,
            FlushToDbTimerPeriodSeconds = opts.FlushToDbTimerPeriodSeconds,
            UseDistributedLock = opts.UseDistributedLock
        });
    }

    [Authorize(ChatAdminPermissions.MessageReports.Flush)]
    public virtual Task<bool> FlushAsync([Required] MessageReportTypes type, long? dateBucket = null)
    {
        return MessageReportManager.FlushAsync(type, dateBucket);
    }
}
