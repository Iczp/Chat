using IczpNet.AbpCommons.Dtos;
using IczpNet.Chat.QrLogins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/qr-login")]
public class QrLoginController(IHubContext<QrLoginHub, IQrLoginClient> hubContext,
    IQrLoginManager qrLoginManager) : ChatController
{
    protected IHubContext<QrLoginHub, IQrLoginClient> HubContext { get; } = hubContext;
    public IQrLoginManager QrLoginManager { get; } = qrLoginManager;

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
        return await QrLoginManager.GenerateAsync(connectionId);
    }

    /// <summary>
    /// 扫码结果
    /// </summary>
    /// <param name="qrText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("scan")]
    [Authorize]
    public async Task<GenerateInfo> ScanAsync([Required] string qrText)
    {
        var res = await QrLoginManager.ScanAsync(qrText);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = "scanned",
            Payload = res
        });
        return res;
    }

    /// <summary>
    /// 授权登录
    /// </summary>
    /// <param name="qrText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("grant")]
    [Authorize]
    public async Task<GrantedInfo> GrantAsync([Required] string qrText)
    {
        var res = await QrLoginManager.GrantAsync(qrText);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = "granted",
            Payload = res
        });
        return res;
    }

    /// <summary>
    /// 授权登录
    /// </summary>
    /// <param name="qrText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("reject")]
    [Authorize]
    public async Task<RejectInfo> RejectAsync([Required] string qrText)
    {
        var res = await QrLoginManager.RejectAsync(qrText);

        await HubContext.Clients.Clients([res.ConnectionId]).ReceivedMessage(new LoginCommandPayload()
        {
            Command = "rejected",
            Payload = res
        });
        return res;
    }
}
