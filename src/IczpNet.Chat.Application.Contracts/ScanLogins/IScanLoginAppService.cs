using System.Threading.Tasks;

namespace IczpNet.Chat.ScanLogins;

public interface IScanLoginAppService
{

    /// <summary>
    /// 生成登录码
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    Task<GeneratedDto> GenerateAsync(string connectionId, string state);

    /// <summary>
    /// 扫码结果
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    Task<ScannedDto> ScanAsync(string scanText);

    /// <summary>
    /// 授权登录
    /// </summary>
    /// <param name="scanText"></param>
    /// <returns></returns>
    Task GrantAsync(string scanText);

    /// <summary>
    /// 拒绝授权
    /// </summary>
    /// <param name="scanText"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task RejectAsync(string scanText, string reason);

    /// <summary>
    /// 拒绝授权
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task CancelAsync(string connectionId, string reason);
}
