﻿using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using IczpNet.AbpCommons.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters;

public class SessionUnitCounterManager : DomainService, ISessionUnitCounterManager
{

    protected ISessionUnitCounterRepository Repository { get; set; }

    public SessionUnitCounterManager(ISessionUnitCounterRepository repository)
    {
        Repository = repository;
    }

    /// <inheritdoc/>
    public async Task<int> IncremenetAsync(SessionUnitCounterArgs args)
    {
        Logger.LogInformation($"Incremenet args:{args},starting.....................................");

        var stopwatch = Stopwatch.StartNew();

        var counter = new List<int>();

        if (args.IsPrivate)
        {
            var count = await Repository.IncrementPrivateBadgeAndUpdateLastMessageIdAsync(
                sessionId: args.SessionId,
                lastMessageId: args.LastMessageId,
                messageCreationTime: args.MessageCreationTime,
                senderSessionUnitId: args.SenderSessionUnitId,
                destinationSessionUnitIdList: args.PrivateBadgeSessionUnitIdList);

            Logger.LogInformation($"IncrementPrivateBadgeAndUpdateLastMessageId count:{count}");

            counter.Add(count);
        }
        else
        {
            var count = await Repository.IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(
                sessionId: args.SessionId,
                lastMessageId: args.LastMessageId,
                messageCreationTime: args.MessageCreationTime,
                senderSessionUnitId: args.SenderSessionUnitId,
                isRemindAll: args.IsRemindAll);

            Logger.LogInformation($"IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync count:{count}");

            counter.Add(count);
        }

        if (args.RemindSessionUnitIdList.IsAny())
        {
            var count = await Repository.IncrementRemindMeCountAsync(
                sessionId: args.SessionId,
                messageCreationTime: args.MessageCreationTime,
                destinationSessionUnitIdList: args.RemindSessionUnitIdList);

            Logger.LogInformation($"IncrementRemindMeCountAsync count:{count}");

            counter.Add(count);
        }

        if (args.FollowingSessionUnitIdList.IsAny())
        {
            var count = await Repository.IncrementFollowingCountAsync(
                sessionId: args.SessionId,
                messageCreationTime: args.MessageCreationTime,
                destinationSessionUnitIdList: args.FollowingSessionUnitIdList);

            Logger.LogInformation($"IncrementFollowingCountAsync count:{count}");

            counter.Add(count);
        }

        stopwatch.Stop();

        var totalCount = counter.Sum();

        Logger.LogInformation($"Incremenet totalCount:{totalCount}, stopwatch: {stopwatch.ElapsedMilliseconds}ms.");

        return totalCount;
    }
}
