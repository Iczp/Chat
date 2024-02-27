using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections;

public abstract class MessageContentAttachmentsInfoBase: MessageContentInfoBase
{
    /// <summary>
    /// BlobId
    /// </summary>
    public virtual Guid? BlobId { get; set; }

    /// <summary>
    /// 文件地址
    /// </summary>
    //[Required(ErrorMessage = "文件地址(必填)")]
    [StringLength(500)]
    public virtual string Url { get; set; }

    /// <summary>
    /// ContentType
    /// </summary>
    [StringLength(100)]
    //[Index]
    public virtual string ContentType { get; set; }

    /// <summary>
    /// 大小 ContentLength(Size)
    /// </summary>
    public virtual long? ContentLength { get; set; }

    /// <summary>
    /// 文件后缀名
    /// </summary>
    [StringLength(10)]
    //[Index]
    public virtual string Suffix { get; set; }
}
