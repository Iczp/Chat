using IczpNet.Chat.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 同账号多身份附件复用幂等记录（以 UserId + SessionUnitId + SourcePrimaryBlobId + ClientUploadId 唯一）
/// </summary>
public class BlobReuseRecord : BaseEntity<Guid>
{
    public virtual Guid UserId { get; protected set; }

    public virtual Guid SessionUnitId { get; protected set; }

    public virtual Guid SourcePrimaryBlobId { get; protected set; }

    [MaxLength(64)]
    [Required]
    public virtual string ClientUploadId { get; protected set; }

    [MaxLength(64)]
    [Required]
    public virtual string FileNameDigest { get; protected set; }

    public virtual Guid PrimaryBlobId { get; protected set; }

    [MaxLength(4000)]
    [Required]
    public virtual string ResultJson { get; protected set; }

    public virtual DateTime? ExpireTime { get; protected set; }

    protected BlobReuseRecord() { }

    public BlobReuseRecord(
        Guid id,
        Guid userId,
        Guid sessionUnitId,
        Guid sourcePrimaryBlobId,
        string clientUploadId,
        string fileNameDigest,
        Guid primaryBlobId,
        string resultJson,
        DateTime? expireTime) : base(id)
    {
        UserId = userId;
        SessionUnitId = sessionUnitId;
        SourcePrimaryBlobId = sourcePrimaryBlobId;
        ClientUploadId = clientUploadId;
        FileNameDigest = fileNameDigest;
        PrimaryBlobId = primaryBlobId;
        ResultJson = resultJson;
        ExpireTime = expireTime;
    }
}
