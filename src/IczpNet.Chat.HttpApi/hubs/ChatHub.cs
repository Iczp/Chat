using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace IczpNet.Chat.hubs;

public class ChatHub(
    IIdentityUserRepository identityUserRepository,
    ILookupNormalizer lookupNormalizer) : AbpHub
{
    private readonly IIdentityUserRepository _identityUserRepository = identityUserRepository;
    private readonly ILookupNormalizer _lookupNormalizer = lookupNormalizer;



    public override Task OnConnectedAsync()
    {
        Logger.LogInformation($"A client connected to the chat hub,ConnectionId:{Context.ConnectionId}.UserName: {CurrentUser.UserName}");
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
