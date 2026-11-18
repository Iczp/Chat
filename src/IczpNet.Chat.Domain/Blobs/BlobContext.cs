using System;

namespace IczpNet.Chat.Blobs;

public class BlobContext
{
    /// <summary>
    /// 
    /// </summary>
    public string Container { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Folder { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid BlobId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid SessionUnitId { get; set; }

    /// <summary>
    /// ChatObjectId（由 SessionUnitId 解析）
    /// </summary>
    public long? ChatObjectId { get; set; }

    /// <summary>
    /// 媒体类型
    /// </summary>
    public AttachmentMediaType? MediaType { get; set; }

    /// <summary>
    /// 附件角色
    /// </summary>
    public BlobRoles BlobRole { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string FileName { get; set; }
}
