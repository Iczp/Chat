using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs.Validation;

/// <summary>
/// 附件上传前置审查验证器（遵循 ABP 多实现注入规约）
/// </summary>
public interface IAttachmentUploadValidator
{
    /// <summary>
    /// 执行顺序（越小越先执行）
    /// </summary>
    int Order { get; }

    /// <summary>
    /// 执行前置审查，不通过则抛出 BusinessException
    /// </summary>
    Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default);
}

public interface IAttachmentUploadValidatorManager
{
    Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default);
}
