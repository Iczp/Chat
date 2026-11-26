using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Blobs;

[Index(nameof(Container))]
[Index(nameof(Name))]
[Index(nameof(MimeType))]
[Index(nameof(Suffix))]
[Index(nameof(UploaderChatObjectId))]
[Index(nameof(ParentBlobId))]
[Index(nameof(Status), nameof(ExpireTime))]
[Index(nameof(StorageObjectId))]
public class Blob : BaseEntity<Guid>, IIsPublic, IIsStatic
{
    /// <summary>
    /// 容器
    /// </summary>
    [MaxLength(50)]
    [Required]
    public virtual string Container { get; protected set; }

    /// <summary>
    /// Blob名称
    /// </summary>
    [MaxLength(500)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    [MaxLength(256)]
    public virtual string FileName { get; set; }

    /// <summary>
    /// 大小
    /// </summary>
    public virtual long FileSize { get; set; }

    /// <summary>
    /// 类型 ContentType
    /// </summary>
    [MaxLength(100)]
    public virtual string MimeType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    [MaxLength(10)]
    public virtual string Suffix { get; set; }

    /// <summary>
    /// IsPublic
    /// </summary>
    public virtual bool IsPublic { get; set; }

    /// <summary>
    /// IsStatic
    /// </summary>
    public virtual bool IsStatic { get; set; }

    /// <summary>
    /// 上传者 ChatObjectId（由 SessionUnitId 解析）
    /// </summary>
    public virtual long? UploaderChatObjectId { get; set; }

    /// <summary>
    /// 上传时的 SessionUnitId（仅用于审计溯源）
    /// </summary>
    public virtual Guid? UploadSessionUnitId { get; set; }

    /// <summary>
    /// 上传时的 SessionId（仅用于审计溯源）
    /// </summary>
    public virtual Guid? UploadSessionId { get; set; }

    /// <summary>
    /// 父级 BlobId（衍生文件指向主文件）
    /// </summary>
    public virtual Guid? ParentBlobId { get; set; }

    /// <summary>
    /// 在附件组中的角色
    /// </summary>
    public virtual BlobRoles Role { get; set; }

    /// <summary>
    /// 生命周期状态
    /// </summary>
    public virtual BlobStatus Status { get; set; }

    /// <summary>
    /// 临时草稿过期时间
    /// </summary>
    public virtual DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 消息引用计数
    /// </summary>
    public virtual int RefCount { get; set; }

    /// <summary>
    /// 物理去重存储对象 Id（历史未迁移数据可为空，回退按 Container + Name 读取）
    /// </summary>
    public virtual Guid? StorageObjectId { get; set; }

    [ForeignKey(nameof(StorageObjectId))]
    public virtual BlobObject StorageObject { get; protected internal set; }

    /// <summary>
    /// 内容
    /// </summary>
    [NotMapped]
    public virtual byte[] Bytes { get; set; }

    public virtual BlobContent Content { get; set; } = new BlobContent();

    public Blob() { }

    public Blob(Guid id, string container) : base(id)
    {
        Container = container;
    }

    public virtual void SetStorageObject(BlobObject storageObject)
    {
        StorageObject = storageObject;
        StorageObjectId = storageObject?.Id;
    }
}
