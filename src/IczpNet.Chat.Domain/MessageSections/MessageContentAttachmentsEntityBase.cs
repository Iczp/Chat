using IczpNet.Chat.Blobs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections;

public abstract class MessageContentAttachmentsEntityBase : MessageContentEntityBase, IAttachments
{

    /// <summary>
    /// BlobId
    /// </summary>
    public virtual Guid? BlobId { get; set; }

    /// <summary>
    /// Blob
    /// </summary>
    [ForeignKey(nameof(BlobId))]
    public virtual Blob Blob { get; protected set; }

    /// <summary>
    /// 文件地址
    /// </summary>
    //[Required(ErrorMessage = "文件地址(必填)")]
    [StringLength(500)]
    public virtual string Url { get; set; }

    /// <summary>
    /// 
    /// 
    /// ContentType
    /// </summary>
    [StringLength(100)]
    //[Index]
    public virtual string ContentType { get; set; }

    /// <summary>
    /// 大小 Size(Size)
    /// </summary>
    public virtual long? Size { get; set; }

    /// <summary>
    /// 文件后缀名
    /// </summary>
    [StringLength(10)]
    //[Index]
    public virtual string Suffix { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [StringLength(500)]
    //[Index]
    public virtual string Description { get; set; }

    protected MessageContentAttachmentsEntityBase() { }

    protected MessageContentAttachmentsEntityBase(Guid id) : base(id) { }

}
