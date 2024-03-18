using System;

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
    public virtual string Url { get; set; }

    /// <summary>
    /// FileName
    /// </summary>
    public virtual string FileName { get; set; }

    /// <summary>
    /// ContentType
    /// </summary>
    public virtual string ContentType { get; set; }

    /// <summary>
    /// 大小 Size(Size)
    /// </summary>
    public virtual long? Size { get; set; }

    /// <summary>
    /// 文件后缀名
    /// </summary>
    public virtual string Suffix { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public virtual string Description { get; set; }
}
