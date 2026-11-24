using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.TextTemplates;
using IczpNet.Chat.Ulids;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.Announcements;

public class ScheduledAnnouncementWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ScheduledAnnouncementWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
    {
        Timer.Period = 60_000; // 每 60 秒检查一次到期定时公告
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();
        var announcementRepo = workerContext.ServiceProvider.GetRequiredService<ISessionAnnouncementRepository>();
        var announcementManager = workerContext.ServiceProvider.GetRequiredService<ISessionAnnouncementManager>();
        var sessionUnitRepo = workerContext.ServiceProvider.GetRequiredService<ISessionUnitRepository>();
        var messageManager = workerContext.ServiceProvider.GetRequiredService<IMessageManager>();
        var objectMapper = workerContext.ServiceProvider.GetRequiredService<IObjectMapper>();
        var ulidGenerator = workerContext.ServiceProvider.GetRequiredService<IUlidGenerator>();
        var distributedLock = workerContext.ServiceProvider.GetRequiredService<IAbpDistributedLock>();
        var uowManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        await using var lockHandle = await distributedLock.TryAcquireAsync(
            "Chat_ScheduledAnnouncementWorker",
            TimeSpan.FromSeconds(5));
        if (lockHandle == null)
        {
            return;
        }

        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        var dueList = await announcementRepo.GetDueScheduledAsync(clock.Now, maxCount: 50);
        if (dueList.Count == 0)
        {
            return;
        }

        foreach (var item in dueList)
        {
            try
            {
                await announcementManager.PublishAsync(item);
                var publisherUnit = await sessionUnitRepo.FindAsync(item.PublisherSessionUnitId);
                if (publisherUnit != null)
                {
                    var unitCache = objectMapper.Map<SessionUnit, SessionUnitCacheItem>(publisherUnit);
                    var publisherName = !unitCache.MemberName.IsNullOrWhiteSpace()
                        ? unitCache.MemberName
                        : unitCache.OwnerName ?? publisherUnit.DisplayName ?? string.Empty;
                    var announcementUrl = SessionTextTemplate.BuildUri(
                        "announcement",
                        new Dictionary<string, string> { ["id"] = item.Id.ToString() });
                    var content = new CmdContentInfo
                    {
                        Cmd = "SessionAnnouncement",
                        Text = new TextTemplate("{publisher} 发布了新公告：《{announcement}》")
                            .WithData("publisher", SessionTextTemplate.SessionUnit(unitCache.Id, publisherName))
                            .WithData("announcement", SessionTextTemplate.Announcement(item.Id, item.Title))
                            .ToString(),
                        Url = announcementUrl
                    };
                    var msg = await messageManager.CreateAnnouncementCommandInCurrentTransactionAsync(
                        unitCache, content, ulidGenerator.Generate());
                    item.SetLinkedMessageId(msg.Id);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "发布定时公告失败: {AnnouncementId}", item.Id);
            }
        }

        await uow.CompleteAsync();
    }
}
