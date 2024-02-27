using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Blobs.Dtos;

public class BlobSimpleDto : EntityDto<Guid>
{
    /// <summary>
    /// 容器
    /// </summary>
    [Required]
    public virtual string Container { get; protected set; }

    /// <summary>
    /// Blob名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public virtual string FileName { get; set; }

    /// <summary>
    /// 大小
    /// </summary>
    public virtual long FileSize { get; set; }

    /// <summary>
    /// 类型 ContentType
    /// </summary>
    public virtual string MimeType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public virtual string Suffix { get; set; }

    /// <summary>
    /// IsPublic
    /// </summary>
    public virtual bool IsPublic { get; set; }

    /// <summary>
    /// IsStatic
    /// </summary>
    public virtual bool IsStatic { get; set; }

}
