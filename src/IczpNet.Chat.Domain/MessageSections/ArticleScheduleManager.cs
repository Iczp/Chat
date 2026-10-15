using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Threading;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.MessageSections;

// The Chat record is authoritative; Quartz is only a durable execution queue.
public class ArticleScheduleManager(
    IRepository<ScheduledArticleMessage, Guid> repository,
    IAbpDistributedLock distributedLock,
    IUnitOfWorkManager unitOfWorkManager,
    IBackgroundJobManager jobs,
    ISessionUnitManager units,
    IRepository<ChatObject, long> accountRepository,
    IArticleScheduleAuthorization authorization,
    ICurrentPrincipalAccessor principalAccessor,
    IMessageSender sender,
    ICancellationTokenProvider cancellationTokenProvider,
    ICurrentTenant currentTenant,
    IConfiguration configuration) : DomainService
{
    public void CheckEnabled()
    {
        if (distributedLock is NullAbpDistributedLock)
            throw new UserFriendlyException("定时发送暂不可用，请联系管理员。");
        if (!configuration.GetValue("Chat:ArticleSchedules:Enabled", false))
            throw new UserFriendlyException("定时文章发送尚未启用，请联系管理员完成服务端配置。");
    }

    public async Task EnqueueAsync(ScheduledArticleMessage task)
    {
        var delay = task.ScheduledAt - Clock.Now;
        await jobs.EnqueueAsync(new ArticleScheduleJobArgs { Id = task.Id, Version = task.Version },
            delay: delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
    }

    public async Task ExecuteAsync(ArticleScheduleJobArgs args)
    {
        CheckEnabled();
        await using var handle = await distributedLock.TryAcquireAsync($"Chat:ArticleSchedule:{args.Id:N}", TimeSpan.FromSeconds(10), cancellationTokenProvider.Token);
        if (handle == null) throw new UserFriendlyException("任务正在处理，请稍后重试。");
        ScheduledArticleMessage task;
        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            task = await repository.FindAsync(args.Id);
            if (task == null || task.Version != args.Version || task.Status is ArticleScheduleStatus.Sent or ArticleScheduleStatus.Cancelled ||
                task.ScheduledAt > Clock.Now)
                return;
            if (task.Attempts >= 5)
            {
                if (task.Status == ArticleScheduleStatus.Running)
                {
                    task.Fail("执行中断次数过多，请手动重试。");
                    await repository.UpdateAsync(task);
                    await uow.CompleteAsync();
                }
                return;
            }
            task.Start(Clock.Now.AddMinutes(5));
            await repository.UpdateAsync(task);
            await uow.CompleteAsync();
        }
        try
        {
            using var tenantScope = currentTenant.Change(task.RequestedByTenantId);
            var unit = await units.GetCacheAsync(task.SenderSessionUnitId);
            var account = await accountRepository.GetAsync(task.OwnerId);
            if (unit.OwnerId != task.OwnerId || !unit.IsEnabled || !account.IsEnabled ||
                (unit.OwnerObjectType != ChatObjectTypeEnums.Official && unit.OwnerObjectType != ChatObjectTypeEnums.Subscription))
                throw new UserFriendlyException("发送主体或目标会话已停用。");
            var principal = await authorization.AuthorizeAsync(task.RequestedBy, task.OwnerId);
            using (principalAccessor.Change(principal))
            {
                var input = JsonSerializer.Deserialize<MessageInput<ArticleContentInput>>(task.InputJson);
                input.ClientMessageId = new Ulid(task.Id).ToString();
                var result = await sender.SendArticleContentMessageAsync(unit, input);
                using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
                var current = await repository.GetAsync(task.Id);
                current.Complete(result.Id, Clock.Now);
                await repository.UpdateAsync(current);
                await uow.CompleteAsync();
            }
        }
        catch (Exception ex)
        {
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var current = await repository.GetAsync(task.Id);
            if (current.Status == ArticleScheduleStatus.Sent) return;
            // Avoid persisting provider/database details or credentials in a UI-visible error.
            current.Fail(ex is UserFriendlyException ? ex.Message : "发送失败，请查看服务端日志或重试。");
            await repository.UpdateAsync(current);
            await uow.CompleteAsync();
            Logger.LogError(ex, "Scheduled article {TaskId} failed", task.Id);
            throw;
        }
    }

    public async Task RecoverAsync()
    {
        CheckEnabled();
        await using var handle = await distributedLock.TryAcquireAsync("Chat:ArticleSchedule:Dispatch", TimeSpan.Zero, cancellationTokenProvider.Token);
        if (handle == null) return;
        using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var now = Clock.Now;
        var query = (await repository.GetQueryableAsync()).Where(x =>
            (x.Status == ArticleScheduleStatus.Pending || x.Status == ArticleScheduleStatus.Running) &&
            x.ScheduledAt <= now && (x.LeaseUntil == null || x.LeaseUntil < now));
        var tasks = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.ScheduledAt).ThenBy(x => x.Id).Take(50), cancellationTokenProvider.Token);
        foreach (var task in tasks)
        {
            await EnqueueAsync(task);
            task.MarkDispatched(now.AddMinutes(5));
            await repository.UpdateAsync(task);
        }
        await uow.CompleteAsync();
    }
}
