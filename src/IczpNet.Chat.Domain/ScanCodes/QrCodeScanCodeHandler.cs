using IczpNet.Chat.QrCodes;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Json;

namespace IczpNet.Chat.ScanCodes;

/// <summary>
/// 名片、群邀请与通用业务二维码扫码处理器
/// </summary>
public class QrCodeScanCodeHandler(
    IQrCodeManager qrCodeManager,
    IJsonSerializer jsonSerializer) : DomainService, IScanCodeHandler, ITransientDependency
{
    protected IQrCodeManager QrCodeManager { get; } = qrCodeManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    public virtual async Task<ScanHandlerResult> HandleAsync(ScanCode scanCode)
    {
        if (!scanCode.IsQrCode())
        {
            return ScanHandlerResult.Fail("非二维码");
        }

        long? currentChatObjectId = null;
        if (scanCode.ExtraProperties != null &&
            scanCode.ExtraProperties.TryGetValue(ScanCodeManager.CurrentChatObjectIdProperty, out var rawObj) &&
            rawObj != null &&
            long.TryParse(rawObj.ToString(), out var parsedId))
        {
            currentChatObjectId = parsedId;
        }

        var data = await QrCodeManager.ResolveScanAsync(scanCode.Content, currentChatObjectId);
        if (data == null)
        {
            return ScanHandlerResult.Fail("非名片或群聊二维码");
        }

        var payload = JsonSerializer.Serialize(data);
        var message = data.IsValid ? "识别成功" : (data.InvalidReason ?? "二维码无效");

        return ScanHandlerResult.Ok(data.Action, message, payload);
    }
}
