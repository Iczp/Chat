using IczpNet.Pusher.Models;
using System.Threading.Tasks;
using System.Linq;
using Volo.Abp.Uow;
using IczpNet.Pusher.Commands;
using IczpNet.AbpCommons;
using NUglify.Helpers;
using Microsoft.Extensions.Logging;

namespace IczpNet.Chat.CommandHandlers;


[Command(CommandConsts.SessionRequest)]
public class SessionRequestCommandHandler : CommandHandlerBase
{
    public SessionRequestCommandHandler() { }

    //[Audited]
    public override async Task HanderAsync(ChannelMessagePayload commandPayload)
    {
        await Task.CompletedTask;

        using var uow = UnitOfWorkManager.Begin();

        var ignoreConnections = commandPayload.IgnoreConnections;

        Logger.LogInformation($"ChannelMessagePayload.Command:{commandPayload.Command}");

        Assert.NotNull(commandPayload.CacheKey.IsNullOrWhiteSpace(), "CacheKey is null.");

        Assert.If(!commandPayload.Command.Any(), "Commands is null.");


    }
}
