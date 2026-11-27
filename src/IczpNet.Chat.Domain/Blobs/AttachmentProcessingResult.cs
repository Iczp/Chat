using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs;

public enum AttachmentProcessingStatus
{
    Pending = 0,
    Processing = 1,
    Ready = 2,
    Failed = 3,
    Invalidated = 4
}

/// <summary>
/// 聊天附件处理结果索引（按 StorageScope + Container + SourceSha256 + SourceFileSize + MediaType + OutputRole + RecipeHash 唯一）
/// 弱引用指向 BlobObject，不延长物理文件生命周期。
/// </summary>
[Index(
    nameof(StorageScope),
    nameof(Container),
    nameof(SourceSha256),
    nameof(SourceFileSize),
    nameof(MediaType),
    nameof(OutputRole),
    nameof(RecipeHash),
    IsUnique = true)]
[Index(nameof(OutputStorageObjectId))]
[Index(nameof(Status), nameof(LeaseExpireTime))]
public class AttachmentProcessingResult : BaseEntity<Guid>
{
    [MaxLength(64)]
    [Required]
    public virtual string StorageScope { get; protected set; }

    [MaxLength(50)]
    [Required]
    public virtual string Container { get; protected set; }

    [MaxLength(64)]
    [Required]
    public virtual string SourceSha256 { get; protected set; }

    public virtual long SourceFileSize { get; protected set; }

    public virtual AttachmentMediaType MediaType { get; protected set; }

    public virtual BlobRoles OutputRole { get; protected set; }

    [MaxLength(64)]
    [Required]
    public virtual string RecipeHash { get; protected set; }

    public virtual AttachmentProcessingStatus Status { get; set; } = AttachmentProcessingStatus.Pending;

    /// <summary>
    /// 成品物理对象 ID（弱引用，不增加 BlobObject.RefCount）
    /// </summary>
    public virtual Guid? OutputStorageObjectId { get; set; }

    [MaxLength(128)]
    public virtual string OutputMimeType { get; set; }

    [MaxLength(16)]
    public virtual string OutputSuffix { get; set; }

    public virtual long? OutputFileSize { get; set; }

    public virtual int? Width { get; set; }

    public virtual int? Height { get; set; }

    public virtual int? DurationMs { get; set; }

    [MaxLength(2000)]
    public virtual string MetadataJson { get; set; }

    public virtual long Version { get; set; }

    [MaxLength(128)]
    public virtual string LeaseOwner { get; set; }

    public virtual DateTime? LeaseExpireTime { get; set; }

    public virtual int RetryCount { get; set; }

    public virtual DateTime? NextRetryTime { get; set; }

    [MaxLength(1024)]
    public virtual string LastError { get; set; }

    public virtual DateTime? LastVerifiedTime { get; set; }

    protected AttachmentProcessingResult() { }

    public AttachmentProcessingResult(
        Guid id,
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash) : base(id)
    {
        StorageScope = storageScope;
        Container = container;
        SourceSha256 = sourceSha256;
        SourceFileSize = sourceFileSize;
        MediaType = mediaType;
        OutputRole = outputRole;
        RecipeHash = recipeHash;
        Status = AttachmentProcessingStatus.Pending;
        Version = 1;
    }
}
