using IczpNet.AbpCommons;
using IczpNet.Pusher.Models;
using Microsoft.Extensions.Logging;
using NUglify.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace IczpNet.Chat.CommandHandlers;

public abstract class SessionIdCommandHandler : CommandHandlerBase
{
    public SessionIdCommandHandler() { }

    //[Audited]
    public override async Task HanderAsync(ChannelMessagePayload commandPayload)
    {
        using var uow = UnitOfWorkManager.Begin();

        var ignoreConnections = commandPayload.IgnoreConnections;

        Logger.LogInformation($"ChannelMessagePayload.Command:{commandPayload.Command}");

        Assert.NotNull(commandPayload.CacheKey.IsNullOrWhiteSpace(), "CacheKey is null.");

        Assert.If(commandPayload.Command.Length == 0, "Commands is null.");

        var sessionUnitInfoList = await SessionUnitManager.GetCacheListAsync(commandPayload.CacheKey);

        if (sessionUnitInfoList == null)
        {
            Logger.LogWarning($"sessionUnitInfoList is null, commandPayload.CacheKey:{commandPayload.CacheKey}");
            return;
        }

        Logger.LogInformation($"Target session unit count:{sessionUnitInfoList.Count}");

        var chatObjectIdList = sessionUnitInfoList
            .Where(x => x != null)
            //.Where(x => x.ServiceStatus == ServiceStatus.Online)
            .Select(x => x.OwnerId)
            .ToList();

        var onlineList = (await PoolsManager.GetAllAsync())
            .Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .ToList();

        Logger.LogInformation($"Online count:{onlineList.Count}");

        //
        foreach (var onlineSessionInfo in onlineList)
        {
            if (ignoreConnections != null && ignoreConnections.Any(x => x == onlineSessionInfo.ConnectionId))
            {
                continue;
            }

            //var sessionUnitCaches = sessionUnitInfoList.Where(x=> chatObjectIdList.Contains(x.OwnerId)).ToList();

            var units = onlineSessionInfo.ChatObjectIdList
                .Where(chatObjectIdList.Contains)
                .Select(chatObjectId => new ScopeUnit
                {
                    ChatObjectId = chatObjectId,
                    SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                }).ToList();

            var message = JsonSerializer.Serialize(new PushPayload()
            {
                AppUserId = onlineSessionInfo.AppUserId,
                Scopes = units,//sessionUnitCaches.Select(x=>x as object).ToList(),
                //Caches = sessionUnitCaches,
                Command = commandPayload.Command,
                Payload = commandPayload.Payload,
            });

            await PoolsManager.SendMessageAsync(onlineSessionInfo, message);
        }
    }
}
