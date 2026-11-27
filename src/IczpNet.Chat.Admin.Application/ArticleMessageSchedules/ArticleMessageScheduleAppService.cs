using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ArticleMessageSchedules.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.ArticleMessageSchedules;

/// <summary>
/// 定时文章消息调度管理服务
/// </summary>
[Authorize(ChatAdminPermissions.ArticleMessages.Default)]
[ExposeServices(typeof(ArticleMessageScheduleAppService), typeof(IArticleMessageScheduleAdminAppService))]
public class ArticleMessageScheduleAppService(IArticleScheduleAdminManager scheduleAdminManager)
    : ApplicationService, IArticleMessageScheduleAdminAppService
{
    protected IArticleScheduleAdminManager ScheduleAdminManager { get; } = scheduleAdminManager;

    [Authorize(ChatAdminPermissions.ArticleMessages.Schedule)]
    public virtual async Task<ArticleMessageScheduleAdminDto> CreateAsync(
        ArticleMessageScheduleAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("请先登录。");
        }

        var task = await ScheduleAdminManager.CreateAdminAsync(
            input.Id,
            input.SenderSessionUnitId,
            input.ScheduledAt,
            input.Message,
            CurrentUser.Id.Value,
            CurrentTenant.Id,
            cancellationToken);

        return MapToDto(task);
    }

    [Authorize(ChatAdminPermissions.ArticleMessages.Manage)]
    public virtual async Task<PagedResultDto<ArticleMessageScheduleAdminDto>> GetListAsync(
        ArticleMessageScheduleAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new ArticleMessageScheduleAdminGetListInput();

        var result = await ScheduleAdminManager.GetAdminListAsync(
            input.SenderSessionUnitId,
            input.OwnerId,
            input.Status,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<ArticleMessageScheduleAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.ArticleMessages.Schedule)]
    public virtual async Task<ArticleMessageScheduleAdminDto> RescheduleAsync(
        Guid id,
        ArticleMessageScheduleAdminTimeInput input,
        CancellationToken cancellationToken = default)
    {
        var task = await ScheduleAdminManager.RescheduleAdminAsync(id, input.ScheduledAt, cancellationToken);
        return MapToDto(task);
    }

    [Authorize(ChatAdminPermissions.ArticleMessages.Schedule)]
    public virtual async Task<ArticleMessageScheduleAdminDto> CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var task = await ScheduleAdminManager.CancelAdminAsync(id, cancellationToken);
        return MapToDto(task);
    }

    [Authorize(ChatAdminPermissions.ArticleMessages.Schedule)]
    public virtual async Task<ArticleMessageScheduleAdminDto> ExecuteNowAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var task = await ScheduleAdminManager.ExecuteNowAdminAsync(id, cancellationToken);
        return MapToDto(task);
    }

    private static ArticleMessageScheduleAdminDto MapToDto(ScheduledArticleMessage task)
    {
        ArticleContentInput content = null;
        if (!string.IsNullOrWhiteSpace(task.InputJson))
        {
            try
            {
                content = JsonSerializer.Deserialize<MessageInput<ArticleContentInput>>(task.InputJson)?.Content;
            }
            catch
            {
                content = null;
            }
        }

        return new ArticleMessageScheduleAdminDto
        {
            Id = task.Id,
            SenderSessionUnitId = task.SenderSessionUnitId,
            OwnerId = task.OwnerId,
            ScheduledAt = task.ScheduledAt,
            Status = task.Status,
            Attempts = task.Attempts,
            SentAt = task.SentAt,
            MessageId = task.MessageId,
            LastError = task.LastError,
            Content = content
        };
    }
}
