using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs.Dtos;

public class BlobCreateInput : BlobUpdateInput
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
    [MaxLength(256)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 大小
    /// </summary>
    public virtual long FileSize { get; set; }

    /// <summary>
    /// 类型 ContentType
    /// </summary>
    [MaxLength(50)]
    public virtual string MimeType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    [MaxLength(10)]
    public virtual string Suffix { get; set; }
}
