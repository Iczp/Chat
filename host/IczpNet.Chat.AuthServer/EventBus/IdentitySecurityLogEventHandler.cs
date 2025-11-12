using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;

namespace IczpNet.Chat.EventBus;

public class IdentitySecurityLogDistributedEventHandler : DomainService, IDistributedEventHandler<EntityCreatedEventData<IdentitySecurityLog>>, ITransientDependency
{

    public  async Task HandleEventAsync(EntityCreatedEventData<IdentitySecurityLog> eventData)
    {
        await Task.Yield();
        Logger.LogInformation($"收到分布事件:${eventData}");

        Console.WriteLine($"用户 {eventData.Entity.UserName} {eventData.Entity.Action}");
    }
}

public class IdentitySecurityLogLocalEventHandler : DomainService, ILocalEventHandler<EntityCreatedEto<IdentitySecurityLog>>, ITransientDependency
{

    public async Task HandleEventAsync(EntityCreatedEto<IdentitySecurityLog> eventData)
    {
        await Task.Yield();
        Logger.LogInformation($"收到 LocalEventHandler 事件:${eventData}");

        Console.WriteLine($"用户 {eventData.Entity.UserName} {eventData.Entity.Action}");
    }
}

public class IdentitySecurityLogEventHandler : DomainService, ITransientDependency,
       IDistributedEventHandler<IdentitySecurityLogContext> // 订阅 IdentitySecurityLogContext
{
    private readonly ILogger<IdentitySecurityLogEventHandler> _logger;

    public IdentitySecurityLogEventHandler(ILogger<IdentitySecurityLogEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleEventAsync(IdentitySecurityLogContext eventData)
    {
        Logger.LogInformation($"收到 LocalEventHandler 事件:${eventData}");

        Console.WriteLine($"用户 {eventData.UserName} {eventData.Action}");

        await Task.Yield();
    }
}