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
