using AutoMapper;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ServiceStates;
using System;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectServiceStutusResolver<TOut> : DomainService, IValueResolver<ChatObject, TOut, ServiceStatus?>, IValueResolver<ChatObject, TOut, List<string>>, ITransientDependency
{
    public IServiceStateManager ServiceStateManager { get; set; }

    public IConnectionPoolManager ConnectionPoolManager { get; set; }

    public ChatObjectServiceStutusResolver() { }

    /// <summary>
    /// 服务状态
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="destMember"></param>
    /// <param name="context"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 设备类型
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="destMember"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<string> Resolve(ChatObject source, TOut destination, List<string> destMember, ResolutionContext context)
    {

        if (source.ObjectType.IsIn(ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Anonymous, ChatObjectTypeEnums.Customer, ChatObjectTypeEnums.ShopWaiter))
        {
            var deviceTypes = ConnectionPoolManager.GetDeviceTypesAsync(source.Id).Result;

            if (deviceTypes!=null)
            {
                return deviceTypes;
            }
            return [];
        }

        if (source.ObjectType.IsIn(ChatObjectTypeEnums.ShopKeeper))
        {
            // 查看子账号是在线
        }

        return [];
    }
}
