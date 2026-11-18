using IczpNet.Chat.QrCodes;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ScanCodes;

/// <summary>
/// 扫码登录格式识别处理器，用于统一扫码入口识别扫码登录二维码并返回模板标识
/// </summary>
public class ScanLoginScanCodeHandler(
    IOptions<QrCodeOptions> options) : DomainService, IScanCodeHandler, ITransientDependency
{
    protected QrCodeOptions Config { get; } = options.Value;

    public virtual Task<ScanHandlerResult> HandleAsync(ScanCode scanCode)
    {
        if (!scanCode.IsQrCode())
        {
            return Task.FromResult(ScanHandlerResult.Fail("非二维码"));
        }

        var templates = new List<string>();
        if (!string.IsNullOrWhiteSpace(Config.ScanLoginTemplate))
        {
            templates.Add(Config.ScanLoginTemplate);
        }
        if (Config.ScanLoginCompatibleTemplates != null)
        {
            templates.AddRange(Config.ScanLoginCompatibleTemplates);
        }

        foreach (var template in templates.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (QrCodeTemplateMatcher.TryMatchTemplate(template, scanCode.Content, out var values) &&
                values.TryGetValue("code", out var code) &&
                !string.IsNullOrWhiteSpace(code))
            {
                return Task.FromResult(ScanHandlerResult.Ok("scan-login", "识别成功", template));
            }
        }

        return Task.FromResult(ScanHandlerResult.Fail($"无法匹配扫码登录模板: {Config.ScanLoginTemplate}"));
    }
}
