using IczpNet.Chat.BaseDtos;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs.Dtos;

public class BlobUpdateInput : BaseInput
{
    /// <summary>
    /// 文件名称
    /// </summary>
    [MaxLength(256)]
    public virtual string FileName { get; set; }

    /// <summary>
    /// IsPublic
    /// </summary>
    public virtual bool IsPublic { get; set; }

    /// <summary>
    /// IsStatic
    /// </summary>
    public virtual bool IsStatic { get; set; }
}
