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
    IScanLoginConnectionPoolManager scanLoginConnectionPoolManager,
    IScanLoginManager scanLoginManager) : ChatController
{
    protected IHubContext<ScanLoginHub, IScanLoginClient> HubContext { get; } = hubContext;
    public IScanLoginConnectionPoolManager ScanLoginConnectionPoolManager { get; } = scanLoginConnectionPoolManager;
    public IScanLoginManager ScanLoginManager { get; } = scanLoginManager;

    [HttpPost]
    [Route("send")]
    public async Task SendMessageAsync(SendMessageInput input)
    {
        await HubContext.Clients.Clients(input.ConnectionIdList).ReceivedMessage(input.CommandPayload);
    }


    [HttpGet]
    [Route("generate")]
    public async Task<GeneratedDto> GenerateAsync([Required] string connectionId, string state)
    {
        var info = await ScanLoginManager.GenerateAsync(connectionId, state);

        return ObjectMapper.Map<GenerateInfo, GeneratedDto>(info);
    }

    /// <summary>
    /// 扫码结果
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("scan")]
    [Authorize]
    public async Task<ScannedDto> ScanAsync([Required] string scanText)
    {
        var info = await ScanLoginManager.ScanAsync(scanText);

        await HubContext.Clients.Clients([info.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Scanned,
            Payload = info
        });

        var output = ObjectMapper.Map<GenerateInfo, ScannedDto>(info);

        output.ConnectionPool = await ScanLoginConnectionPoolManager.GetAsync(info.ConnectionId);

        return output;

    }

    /// <summary>
    /// 授权登录
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("grant")]
    [Authorize]
    public async Task GrantAsync([Required] string scanText)
    {
        var res = await ScanLoginManager.GrantAsync(scanText);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Granted,
            Payload = res
        });
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
    public async Task RejectAsync([Required] string scanText, string reason)
    {
        var res = await ScanLoginManager.RejectAsync(scanText, reason);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Rejected,
            Payload = res
        });
    }

    /// <summary>
    /// 拒绝授权
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("cancel")]
    [Authorize]
    public async Task CancelAsync([Required] string connectionId, string reason)
    {
        var res = await ScanLoginManager.CancelAsync(connectionId, reason);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Cancelled,
            Payload = res
        });
    }
}
