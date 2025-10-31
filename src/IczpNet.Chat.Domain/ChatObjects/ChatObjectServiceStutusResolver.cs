using AutoMapper;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ServiceStates;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectServiceStutusResolver<TOut> : DomainService, IValueResolver<ChatObject, TOut, ServiceStatus?>, ITransientDependency
{
    public IServiceStateManager ServiceStateManager { get; set; }

    public IConnectionPoolManager ConnectionPoolManager { get; set; }

    public ChatObjectServiceStutusResolver() { }

    public ServiceStatus? Resolve(ChatObject source, TOut destination, ServiceStatus? destMember, ResolutionContext context)
    {
        var statues = ServiceStateManager.GetStatusAsync(source.Id).Result;

        if (source.ObjectType.IsIn(ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Anonymous, ChatObjectTypeEnums.Customer, ChatObjectTypeEnums.ShopWaiter))
        {
            var isOnliine = ConnectionPoolManager.IsOnlineAsync(source.Id).Result;

            if (isOnliine)
            {
                return ServiceStatus.Online;
            }
            return ServiceStatus.Offline;
        }

        if (source.ObjectType.IsIn(ChatObjectTypeEnums.ShopKeeper))
        {
            if (statues != null && (int)statues > 0)
            {
                return statues;
            }
            // 查看子账号是在线
        }

        return statues;

    }
}
