using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Connections;
using IczpNet.Pusher.DeviceIds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.hubs;
[Authorize]
public class ChatHub(
    IIdentityUserRepository identityUserRepository,
    IConnectionManager connectionManager,
    IWebClientInfoProvider webClientInfoProvider,
    IChatObjectManager chatObjectManager,
    IBackgroundJobManager backgroundJobManager,
    
    ILookupNormalizer lookupNormalizer) : AbpHub
{
    private readonly IIdentityUserRepository _identityUserRepository = identityUserRepository;
    private readonly ILookupNormalizer _lookupNormalizer = lookupNormalizer;

    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public IWebClientInfoProvider WebClientInfoProvider { get; } = webClientInfoProvider;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    public IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;

    [UnitOfWork]
    public override async Task OnConnectedAsync()
    {
        Logger.LogInformation($"[OnConnected] ConnectionId:{Context.ConnectionId}.UserName: {CurrentUser.UserName}");
        Logger.LogInformation($"[BrowserInfo] {WebClientInfoProvider.BrowserInfo}");
        Logger.LogInformation($"[DeviceInfo] {WebClientInfoProvider.DeviceInfo}");
        Logger.LogInformation($"[ClientIpAddress] {WebClientInfoProvider.ClientIpAddress}");

        var chatObjectIdList = CurrentUser.Id.HasValue
            ? await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value)
            : [];

        await ConnectionManager.CreateAsync(new Connection(Context.ConnectionId, chatObjectIdList)
        {
            AppUserId = CurrentUser.Id,
            IpAddress = WebClientInfoProvider.ClientIpAddress,
            //ServerHostId = "host",
            DeviceId = CurrentUser.GetDeviceId(),
            BrowserInfo = WebClientInfoProvider.BrowserInfo,
            DeviceInfo = WebClientInfoProvider.DeviceInfo,
        });
        await base.OnConnectedAsync();
    }

    [UnitOfWork]
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;
        var userName = CurrentUser.UserName;
        Logger.LogInformation($"[OnDisconnected],ConnectionId:{connectionId}.UserName: {userName}");
        try
        {
            // 注：这里的删除操作可能会被取消，所以需要捕获TaskCanceledException异常
            await ConnectionManager.RemoveAsync(Context.ConnectionId);
        }
        catch (TaskCanceledException)
        {
            //Logger.LogWarning(ex, $"TaskCanceledException while deleting connection {connectionId}. UserName: {userName}");
            Logger.LogWarning($"[{nameof(ConnectionRemoveJob)}] deleting connection {connectionId}. UserName: {userName}");
            // 使用后台任务删除连接
            await BackgroundJobManager.EnqueueAsync(new ConnectionRemoveJobArgs(connectionId));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while deleting connection {connectionId}. UserName: {userName}");
            // 处理其他异常
        }
        await base.OnDisconnectedAsync(exception);
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
