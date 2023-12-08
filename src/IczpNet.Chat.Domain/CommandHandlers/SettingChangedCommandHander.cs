using IczpNet.Pusher.Models;
using System.Threading.Tasks;
using System.Linq;
using Volo.Abp.Uow;
using IczpNet.Pusher.Commands;
using IczpNet.AbpCommons;
using NUglify.Helpers;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.Commands;

namespace IczpNet.Chat.CommandHandlers;


/// <summary>
/// 设置变更的时候推送
/// </summary>
[Command(CommandConsts.SettingChanged)]
public class SettingChangedCommandHander : CommandHandlerBase
{
    public SettingChangedCommandHander() { }

    //[Audited]
    public override async Task HanderAsync(ChannelMessagePayload commandPayload)
    {
        await Task.Yield();

        using var uow = UnitOfWorkManager.Begin();

        var ignoreConnections = commandPayload.IgnoreConnections;

        Logger.LogInformation($"ChannelMessagePayload.Command:{commandPayload.Command}");

        Assert.NotNull(commandPayload.CacheKey.IsNullOrWhiteSpace(), "CacheKey is null.");

        Assert.If(!commandPayload.Command.Any(), "Commands is null.");

        var onlineList = (await ConnectionManager.GetAllAsync())
            //.Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .ToList();


    }
}
