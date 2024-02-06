using AutoMapper;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ServiceStates;
using Microsoft.Extensions.Logging;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectServiceStutusResolver : DomainService, IValueResolver<ChatObject, ChatObjectDto, ServiceStatus?>, ITransientDependency
{
    public IServiceStateManager ServiceStateManager { get; set; }
    public ChatObjectServiceStutusResolver() { }


    public ServiceStatus? Resolve(ChatObject source, ChatObjectDto destination, ServiceStatus? destMember, ResolutionContext context)
    {
        var item = ServiceStateManager.GetAsync(source.Id).Result;

        ServiceStatus? status = ServiceStatus.Offline;

        if (item != null)
        {
            status = item.Status;
        }
        Logger.LogDebug($"ChatObjectId:{source.Id},ServiceStatus:{status}");

        return status;
    }
}
