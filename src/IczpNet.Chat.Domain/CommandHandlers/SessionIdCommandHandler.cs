using IczpNet.Pusher.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Volo.Abp.Uow;
using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using NUglify.Helpers;

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

        Assert.If(!commandPayload.Command.Any(), "Commands is null.");

        var sessionUnitInfoList = await SessionUnitManager.GetCacheListAsync(commandPayload.CacheKey);

        Logger.LogInformation($"Target session unit count:{sessionUnitInfoList.Count}");

        var chatObjectIdList = sessionUnitInfoList
            .Where(x => x != null)
            .Where(x => x.ServiceStatus == ServiceStatus.Normal)
            .Select(x => x.OwnerId)
            .ToList();

        var onlineList = (await ConnectionManager.GetAllAsync())
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
                Scopes = units,
                Command = commandPayload.Command,
                Payload = commandPayload.Payload,
            });

            await ConnectionManager.SendMessageAsync(onlineSessionInfo, message);
        }
    }
}
