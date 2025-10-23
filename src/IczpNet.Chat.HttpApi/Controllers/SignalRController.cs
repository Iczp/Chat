using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.CommandPayloads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/signalR")]
public class SignalRController(IHubContext<ChatHub, IChatClient> hubContext) : ChatController
{
    protected IHubContext<ChatHub, IChatClient> HubContext { get; } = hubContext;
    /// <summary>
    /// 发送消息
    /// </summary>

    [HttpPost]
    [Route("send-message")]
    public async Task SendMessageAsync(string message)
    {
        await HubContext.Clients.All.ReceivedMessage(new CommandPayload()
        {
            Command = "test",
            Payload = message
        });
    }

    [HttpPost]
    [Route("sendTo-connectionId")]
    public async Task SendToConnectionIdAsync(string connectionId, string message)
    {
        await HubContext.Clients.Clients(connectionId).ReceivedMessage(new CommandPayload()
        {
            Command = "test",
            Payload = message
        });
    }

    [HttpPost]
    [Route("sendTo-userId")]
    public async Task SendToUserIdAsync(Guid userId, string message)
    {
        await HubContext.Clients.User(userId.ToString()).ReceivedMessage(new CommandPayload()
        {
            Command = "test",
            Payload = message
        });
    }

    [HttpPost]
    [Route("sendTo-group")]
    public async Task SendToGroupAsync(string group, string message)
    {
        await HubContext.Clients.Group(group).ReceivedMessage(new CommandPayload()
        {
            Command = "test",
            Payload = message
        });
    }
}
