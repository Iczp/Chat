using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs.Validation;

public class AttachmentUploadValidatorManager(
    IEnumerable<IAttachmentUploadValidator> validators) : DomainService, IAttachmentUploadValidatorManager
{
    public virtual async Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default)
    {
        foreach (var validator in validators.OrderBy(x => x.Order))
        {
            await validator.ValidateAsync(context, cancellationToken);
        }
    }
}
