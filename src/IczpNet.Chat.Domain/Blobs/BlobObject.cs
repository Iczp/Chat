using IczpNet.Chat.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 物理存储对象（按 StorageScope + Container + Sha256 + FileSize 去重）
/// </summary>
public class BlobObject : BaseEntity<Guid>
{
    [MaxLength(64)]
    [Required]
    public virtual string StorageScope { get; protected set; }

    [MaxLength(50)]
    [Required]
    public virtual string Container { get; protected set; }

    [MaxLength(500)]
    [Required]
    public virtual string StorageName { get; set; }

    [MaxLength(64)]
    [Required]
    public virtual string Sha256 { get; protected set; }

    public virtual long FileSize { get; protected set; }

    [MaxLength(128)]
    public virtual string MimeType { get; set; }

    public virtual BlobObjectStatus Status { get; set; } = BlobObjectStatus.Pending;

    public virtual long RefCount { get; set; }

    public virtual DateTime? DeletingSince { get; set; }

    public virtual DateTime? LastVerifiedTime { get; set; }

    protected BlobObject() { }

    public BlobObject(
        Guid id,
        string storageScope,
        string container,
        string storageName,
        string sha256,
        long fileSize,
        string mimeType) : base(id)
    {
        StorageScope = storageScope;
        Container = container;
        StorageName = storageName;
        Sha256 = sha256;
        FileSize = fileSize;
        MimeType = mimeType;
        Status = BlobObjectStatus.Pending;
        RefCount = 0;
    }
}
