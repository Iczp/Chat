using IczpNet.Chat.CommandPayloads;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatHubs;

public class Sender(IHubContext<ChatHub, IChatClient> hubContext) : DomainService, ISender
{
    public IHubContext<ChatHub, IChatClient> HubContext { get; } = hubContext;

    public async Task SendAsync(string method, object payload)
    {
        await HubContext.Clients.All.ReceivedMessage(new CommandPayload()
        {
            Command = method,
            Payload = payload
        });
    }
}
