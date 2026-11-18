using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.DistributedEventHandlers;

public abstract class SendToClientDistributedEventHandler<T> : DomainService, IDistributedEventHandler<T>, ITransientDependency
{
    public IOnlineManager OnlineManager => LazyServiceProvider.LazyGetRequiredService<IOnlineManager>();

    public IHubContext<ChatHub, IChatClient> HubContext => LazyServiceProvider.LazyGetRequiredService<IHubContext<ChatHub, IChatClient>>();

    public abstract Task HandleEventAsync(T eventData);

    protected virtual async Task<TResult> MeasureAsync<TResult>(string name, Func<Task<TResult>> func)
    {
        var sw = Stopwatch.StartNew();
        var result = await func();
        Logger.LogInformation("[{FullName}] [{name}] Elapsed Time: {ElapsedMilliseconds} ms", GetType().FullName, name, sw.ElapsedMilliseconds);
        sw.Stop();
        return result;
    }
}
