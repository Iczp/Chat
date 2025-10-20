using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface ICallerContextManager
{
    Task<CallerContext> AddAsync(CallerContext caller);

    Task<CallerContext> AddAsync(HubCallerContext context, OnConnectedEto connectedEto);

    Task<bool> RemoveAsync(string connectionId);

    Task AbortAsync(string connectionId);
}
