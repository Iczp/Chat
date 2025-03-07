using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Hubs;

public class Sender(IHubContext<ChatHub, IChatClient> hubContext) : DomainService, ISender
{
    public IHubContext<ChatHub, IChatClient> HubContext { get; } = hubContext;

    public async Task SendAsync(string method, object payload)
    {
        await HubContext.Clients.All.ReceivedMessage(new IczpNet.Pusher.Models.ChannelMessagePayload()
        {
            Command = method,
            Payload = payload
        });
    }
}
