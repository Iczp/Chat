using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Identity;

namespace IczpNet.Chat.hubs;

public class ChatHub : AbpHub
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly ILookupNormalizer _lookupNormalizer;

    public ChatHub(
        IIdentityUserRepository identityUserRepository,
        ILookupNormalizer lookupNormalizer)
    {
        _identityUserRepository = identityUserRepository;
        _lookupNormalizer = lookupNormalizer;
    }

    public override Task OnConnectedAsync()
    {
        Logger.LogInformation($"A client connected to the chat hub,ConnectionId:{Context.ConnectionId}.");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageAsync(string targetUserName, string message)
    {

        var targetUser = await _identityUserRepository.FindByNormalizedUserNameAsync(_lookupNormalizer.NormalizeName(targetUserName));

        message = $"{CurrentUser.UserName}: {message}";

        await Clients.All.SendAsync("ReceiveMessage", message);
        //await Clients
        //    .User(targetUser.Id.ToString())
        //    .SendAsync("ReceiveMessage", message);
    }
}
