using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 视频 GIF 传播本地事务 Outbox 记录（与 GIF 转码结果同事务提交，由后台 Worker 幂等投递）
/// </summary>
public class VideoGifPropagationOutbox : FullAuditedEntity<Guid>
{
    public virtual long SourceMessageId { get; protected set; }

    public virtual Guid VideoContentId { get; protected set; }

    public virtual Guid GifBlobId { get; protected set; }

    [MaxLength(1024)]
    [Required]
    public virtual string GifUrl { get; protected set; }

    public virtual int RetryCount { get; protected set; }

    public virtual DateTime? NextRetryTime { get; protected set; }

    public virtual DateTime? ProcessedTime { get; protected set; }

    [MaxLength(2000)]
    public virtual string LastError { get; protected set; }

    protected VideoGifPropagationOutbox() { }

    public VideoGifPropagationOutbox(
        Guid id,
        long sourceMessageId,
        Guid videoContentId,
        Guid gifBlobId,
        string gifUrl) : base(id)
    {
        SourceMessageId = sourceMessageId;
        VideoContentId = videoContentId;
        GifBlobId = gifBlobId;
        GifUrl = gifUrl;
        RetryCount = 0;
    }

    public virtual void MarkProcessed(DateTime now)
    {
        ProcessedTime = now;
        NextRetryTime = null;
        LastError = null;
    }

    public virtual void MarkFailed(string error, DateTime nextRetryTime)
    {
        RetryCount++;
        NextRetryTime = nextRetryTime;
        if (!string.IsNullOrEmpty(error) && error.Length > 2000)
        {
            error = error[..2000];
        }
        LastError = error;
    }
}
