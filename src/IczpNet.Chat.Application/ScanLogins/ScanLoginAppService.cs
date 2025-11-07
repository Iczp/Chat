using IczpNet.Chat.BaseAppServices;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.ScanLogins;

[RemoteService(false)]
public class ScanLoginAppService(IScanLoginManager scanLoginManager) : ChatAppService, IScanLoginAppService
{
    public IScanLoginManager ScanLoginManager { get; } = scanLoginManager;

    [HttpGet]
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
    public async Task<ScannedDto> ScanAsync([Required] string scanText)
    {
        var info = await ScanLoginManager.ScanAsync(scanText);

        return ObjectMapper.Map<GenerateInfo, ScannedDto>(info);
    }

    /// <summary>
    /// 授权登录
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task GrantAsync([Required] string scanText)
    {
        await ScanLoginManager.GrantAsync(scanText);
    }

    /// <summary>
    /// 拒绝授权
    /// </summary>
    /// <param name="scanText"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task RejectAsync([Required] string scanText, string reason)
    {
        await ScanLoginManager.RejectAsync(scanText, reason);
    }

    /// <summary>
    /// 拒绝授权
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task CancelAsync([Required] string connectionId, string reason)
    {
        await ScanLoginManager.CancelAsync(connectionId, reason);
    }
}
