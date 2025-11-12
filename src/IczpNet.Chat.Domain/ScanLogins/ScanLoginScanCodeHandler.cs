using IczpNet.Chat.ScanCodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ScanLogins;

public class ScanLoginScanCodeHandler(
    IOptions<ScanLoginOption> options
    ) : DomainService, IScanCodeHandler
{
    public IOptions<ScanLoginOption> Options { get; } = options;
    protected ScanLoginOption Config => Options.Value;

    public async Task<ScanHandlerResult> HandleAsync(ScanCode scanCode)
    {
        if (!scanCode.IsQrCode())
        {
            return ScanHandlerResult.Fail("非二维码");
        }

        var builder = new StringTemplateBuilder(Config.ScanTextTemplate);

        if (!builder.TryToValues(scanCode.Content, out _))
        {
            return ScanHandlerResult.Fail($"无法识别模板: {Config.ScanTextTemplate}");
        }

        Logger.LogInformation($"LoginScanHandler 处理: {scanCode.Content}");

        //await Task.Delay(300); // 模拟耗时操作
        await Task.Yield();

        return ScanHandlerResult.Ok("scan-login", "识别成功");
    }
}
