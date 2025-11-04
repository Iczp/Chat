using IczpNet.Chat.ScanLogins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/scan-login")]
public class ScanLoginController(IHubContext<ScanLoginHub, IScanLoginClient> hubContext,
    IScanLoginManager scanLoginManager) : ChatController
{
    protected IHubContext<ScanLoginHub, IScanLoginClient> HubContext { get; } = hubContext;
    public IScanLoginManager ScanLoginManager { get; } = scanLoginManager;

    [HttpPost]
    [Route("send")]
    public async Task SendMessageAsync(SendMessageInput input)
    {
        await HubContext.Clients.Clients(input.ConnectionIdList).ReceivedMessage(input.CommandPayload);
    }


    [HttpGet]
    [Route("generate")]
    public async Task<GenerateInfo> GenerateAsync([Required] string connectionId)
    {
        return await ScanLoginManager.GenerateAsync(connectionId);
    }

    /// <summary>
    /// 扫码结果
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("scan")]
    [Authorize]
    public async Task<GenerateInfo> ScanAsync([Required] string scanText)
    {
        var res = await ScanLoginManager.ScanAsync(scanText);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Scanned,
            Payload = res
        });
        return res;
    }

    /// <summary>
    /// 授权登录
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("grant")]
    [Authorize]
    public async Task<GrantedInfo> GrantAsync([Required] string scanText)
    {
        var res = await ScanLoginManager.GrantAsync(scanText);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Granted,
            Payload = res
        });
        return res;
    }

    /// <summary>
    /// 拒绝授权
    /// </summary>
    /// <param name="scanText"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("reject")]
    [Authorize]
    public async Task<RejectInfo> RejectAsync([Required] string scanText, string reason)
    {
        var res = await ScanLoginManager.RejectAsync(scanText, reason);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Rejected,
            Payload = res
        });
        return res;
    }
}
