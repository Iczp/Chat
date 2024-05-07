using AutoMapper;

using IczpNet.Chat.Enums;
using IczpNet.Chat.ServiceStates;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectServiceStutusResolver<TOut> : DomainService, IValueResolver<ChatObject, TOut, ServiceStatus?>, ITransientDependency
{
    public IServiceStateManager ServiceStateManager { get; set; }
    //protected IServiceStateManager ServiceStateManager => LazyServiceProvider.LazyGetRequiredService<IServiceStateManager>();
    public ChatObjectServiceStutusResolver() { }

    public ServiceStatus? Resolve(ChatObject source, TOut destination, ServiceStatus? destMember, ResolutionContext context)
    {
        return ServiceStateManager.GetStatusAsync(source.Id).Result ?? ServiceStatus.Offline;
    }
}
