using IczpNet.AbpCommons;
using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;
using IczpNet.Pusher.Models;
using Microsoft.Extensions.Logging;
using NUglify.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace IczpNet.Chat.CommandHandlers;


[Command(CommandConsts.SessionRequest)]
public class SessionRequestCommandHandler : CommandHandlerBase
{
    public SessionRequestCommandHandler() { }

    //[Audited]
    public override async Task HanderAsync(ChannelMessagePayload commandPayload)
    {
        await Task.Yield();

        using var uow = UnitOfWorkManager.Begin();

        var ignoreConnections = commandPayload.IgnoreConnections;

        Logger.LogInformation($"ChannelMessagePayload.Command:{commandPayload.Command}");

        Assert.NotNull(commandPayload.CacheKey.IsNullOrWhiteSpace(), "CacheKey is null.");

        Assert.If(!commandPayload.Command.Any(), "Commands is null.");


    }
}
