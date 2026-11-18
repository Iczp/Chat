using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// Blob 与消息授权引用关系（支持原始发送、跨会话转发、合并转发嵌套授权）
/// </summary>
[Index(nameof(BlobId), nameof(MessageId), IsUnique = true)]
[Index(nameof(MessageId))]
[Index(nameof(SessionId))]
public class BlobMessageRef : BaseEntity<Guid>
{
    public virtual Guid BlobId { get; protected set; }

    [ForeignKey(nameof(BlobId))]
    public virtual Blob Blob { get; protected set; }

    public virtual long MessageId { get; protected set; }

    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { get; protected set; }

    public virtual Guid? SessionId { get; protected set; }

    public virtual BlobRefSourceTypes SourceType { get; protected set; }

    protected BlobMessageRef() { }

    public BlobMessageRef(Guid id, Guid blobId, long messageId, Guid? sessionId, BlobRefSourceTypes sourceType)
        : base(id)
    {
        BlobId = blobId;
        MessageId = messageId;
        SessionId = sessionId;
        SourceType = sourceType;
    }
}
