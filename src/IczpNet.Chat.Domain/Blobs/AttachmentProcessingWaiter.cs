using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;

namespace IczpNet.Chat.Blobs;

public enum AttachmentProcessingWaiterStatus
{
    Pending = 0,
    Completed = 1,
    Skipped = 2,
    Failed = 3
}

/// <summary>
/// 等待异步附件处理结果（如视频 GIF）的消息登记表
/// </summary>
[Index(nameof(MessageId), nameof(OutputRole), IsUnique = true)]
[Index(nameof(ProcessingResultId), nameof(Status))]
public class AttachmentProcessingWaiter : BaseEntity<Guid>
{
    public virtual Guid ProcessingResultId { get; protected set; }

    public virtual long MessageId { get; protected set; }

    public virtual Guid PrimaryBlobId { get; protected set; }

    public virtual BlobRoles OutputRole { get; protected set; }

    public virtual AttachmentProcessingWaiterStatus Status { get; set; } = AttachmentProcessingWaiterStatus.Pending;

    public virtual DateTime? CompletedTime { get; set; }

    protected AttachmentProcessingWaiter() { }

    public AttachmentProcessingWaiter(
        Guid id,
        Guid processingResultId,
        long messageId,
        Guid primaryBlobId,
        BlobRoles outputRole) : base(id)
    {
        ProcessingResultId = processingResultId;
        MessageId = messageId;
        PrimaryBlobId = primaryBlobId;
        OutputRole = outputRole;
        Status = AttachmentProcessingWaiterStatus.Pending;
    }
}
