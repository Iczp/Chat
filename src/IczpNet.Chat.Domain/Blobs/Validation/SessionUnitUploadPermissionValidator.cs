using IczpNet.Chat.Settings;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace IczpNet.Chat.Blobs.Validation;

/// <summary>
/// 校验会话单元是否有发送权限（启用、未被禁言、允许输入）
/// </summary>
public class SessionUnitUploadPermissionValidator(
    ISettingProvider settingProvider,
    IClock clock) : IAttachmentUploadValidator, ITransientDependency
{
    public int Order => 100;

    public async Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default)
    {
        if (!await settingProvider.IsTrueAsync(ChatSettings.IsMessageSenderEnabled))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "MessageSender main switch is off.");
        }

        var unit = context.SessionUnit
            ?? throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "SessionUnit is null.");

        if (!unit.IsEnabled)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Session unit disabled.");
        }

        if (!unit.IsInputEnabled)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Session unit input disabled.");
        }

        if (unit.MuteExpireTime.HasValue && unit.MuteExpireTime > clock.Now)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Session unit has been muted.");
        }
    }
}
