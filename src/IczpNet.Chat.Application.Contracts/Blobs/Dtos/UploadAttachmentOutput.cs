using System;

namespace IczpNet.Chat.Blobs.Dtos;

/// <summary>
/// 独立附件上传返回结果
/// </summary>
public class UploadAttachmentOutput
{
    public Guid BlobId { get; set; }

    public AttachmentMediaType MediaType { get; set; }

    public string Url { get; set; }

    public Guid? ThumbnailBlobId { get; set; }

    public string ThumbnailUrl { get; set; }

    public Guid? OriginalBlobId { get; set; }

    public string OriginalUrl { get; set; }

    public Guid? SnapshotBlobId { get; set; }

    public string SnapshotUrl { get; set; }

    public Guid? SnapshotThumbnailBlobId { get; set; }

    public string SnapshotThumbnailUrl { get; set; }

    public Guid? GifBlobId { get; set; }

    public string GifUrl { get; set; }

    public string FileName { get; set; }

    public string ContentType { get; set; }

    public string Suffix { get; set; }

    public long Size { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    /// <summary>
    /// 时长（毫秒）
    /// </summary>
    public int? Duration { get; set; }

    public string Profile { get; set; }

    public DateTime? ExpireTime { get; set; }
}
