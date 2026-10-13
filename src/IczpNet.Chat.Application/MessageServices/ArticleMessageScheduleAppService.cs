using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageServices;

public class ArticleMessageScheduleAppService(
    IRepository<ScheduledArticleMessage, Guid> repository,
    ArticleScheduleManager manager,
    IAbpDistributedLock distributedLock,
    ICancellationTokenProvider cancellationTokenProvider) : ChatAppService, IArticleMessageScheduleAppService
{
    private async Task<long> CheckSenderAsync(Guid id, bool requireSendable = false)
    {
        manager.CheckEnabled();
        var unit = await SessionUnitManager.GetCacheAsync(id);
        await CheckArticleManagementOrOwnerAsync(unit.OwnerId);
        if ((requireSendable && (!unit.IsEnabled || !unit.IsInputEnabled)) ||
            (unit.OwnerObjectType != ChatObjectTypeEnums.Official && unit.OwnerObjectType != ChatObjectTypeEnums.Subscription))
            throw new UserFriendlyException("请选择已启用的公众号或服务号会话。");
        return unit.OwnerId;
    }
    private DateTime ValidateTime(DateTimeOffset value)
    {
        var time = value.UtcDateTime;
        if (time <= Clock.Now.ToUniversalTime()) throw new UserFriendlyException("计划时间必须晚于当前时间。");
        return time;
    }
    private static ArticleMessageScheduleDto Map(ScheduledArticleMessage task) => new()
    {
        Id = task.Id, SenderSessionUnitId = task.SenderSessionUnitId, ScheduledAt = task.ScheduledAt,
        Status = task.Status, Attempts = task.Attempts, SentAt = task.SentAt, MessageId = task.MessageId,
        LastError = task.LastError, Content = JsonSerializer.Deserialize<MessageInput<ArticleContentInput>>(task.InputJson)?.Content
    };

    [HttpPost("/api/chat/article-message-schedule")]
    public async Task<ArticleMessageScheduleDto> CreateAsync(ArticleMessageScheduleCreateInput input)
    {
        var ownerId = await CheckSenderAsync(input.SenderSessionUnitId, requireSendable: true);
        if (input.Id == Guid.Empty) throw new UserFriendlyException("请提供稳定的任务标识。");
        ArticleContent.Create(input.Message?.Content);
        var json = JsonSerializer.Serialize(input.Message);
        if (json.Length > 32768) throw new UserFriendlyException("内容过长，请减少消息附加参数。");
        await using var handle = await distributedLock.TryAcquireAsync($"Chat:ArticleSchedule:{input.Id:N}", TimeSpan.FromSeconds(10), cancellationTokenProvider.Token);
        if (handle == null) throw new UserFriendlyException("任务正在处理，请稍后重试。");
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var task = await repository.FindAsync(input.Id);
        if (task != null)
        {
            if (task.RequestedBy != CurrentUser.Id || task.SenderSessionUnitId != input.SenderSessionUnitId || task.InputJson != json || task.ScheduledAt != input.ScheduledAt.UtcDateTime)
                throw new UserFriendlyException("任务标识已被使用，请刷新草稿后重试。");
            return Map(task);
        }
        task = new ScheduledArticleMessage(input.Id, input.SenderSessionUnitId, ownerId, CurrentUser.Id.Value, json, ValidateTime(input.ScheduledAt), CurrentTenant.Id);
        await repository.InsertAsync(task);
        await uow.CompleteAsync();
        await manager.EnqueueAsync(task);
        return Map(task);
    }

    [HttpGet("/api/chat/article-message-schedule")]
    public async Task<PagedResultDto<ArticleMessageScheduleDto>> GetListAsync(ArticleMessageScheduleGetListInput input)
    {
        await CheckSenderAsync(input.SenderSessionUnitId);
        if (input.SkipCount > 10000) throw new UserFriendlyException("请缩小查询范围。");
        var query = (await repository.GetQueryableAsync()).Where(x => x.SenderSessionUnitId == input.SenderSessionUnitId);
        var count = await AsyncExecuter.CountAsync(query, cancellationTokenProvider.Token);
        var list = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.Id)
            .Skip(input.SkipCount).Take(Math.Clamp(input.MaxResultCount, 1, 100)), cancellationTokenProvider.Token);
        return new PagedResultDto<ArticleMessageScheduleDto>(count, list.Select(Map).ToList());
    }

    private async Task<ArticleMessageScheduleDto> ChangeAsync(Guid id, DateTime? time)
    {
        manager.CheckEnabled();
        await using var handle = await distributedLock.TryAcquireAsync($"Chat:ArticleSchedule:{id:N}", TimeSpan.FromSeconds(10), cancellationTokenProvider.Token);
        if (handle == null) throw new UserFriendlyException("任务正在执行，请稍后刷新状态。");
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var task = await repository.GetAsync(id);
        await CheckSenderAsync(task.SenderSessionUnitId, requireSendable: time.HasValue);
        if (time.HasValue) task.Reschedule(time.Value); else task.Cancel();
        await repository.UpdateAsync(task);
        await uow.CompleteAsync();
        if (time.HasValue) await manager.EnqueueAsync(task);
        return Map(task);
    }
    [HttpPost("/api/chat/article-message-schedule/{id}/reschedule")]
    public Task<ArticleMessageScheduleDto> RescheduleAsync(Guid id, ArticleMessageScheduleTimeInput input) => ChangeAsync(id, ValidateTime(input.ScheduledAt));
    [HttpPost("/api/chat/article-message-schedule/{id}/cancel")]
    public Task<ArticleMessageScheduleDto> CancelAsync(Guid id) => ChangeAsync(id, null);
    [HttpPost("/api/chat/article-message-schedule/{id}/execute-now")]
    public Task<ArticleMessageScheduleDto> ExecuteNowAsync(Guid id) => ChangeAsync(id, Clock.Now);
}
