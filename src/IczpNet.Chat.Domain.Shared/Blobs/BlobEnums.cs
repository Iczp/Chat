namespace IczpNet.Chat.Blobs;

/// <summary>
/// 附件媒体类型
/// </summary>
public enum AttachmentMediaType
{
    File = 0,
    Image = 1,
    Sound = 2,
    Video = 3,
    Snapshot = 4,
}

/// <summary>
/// Blob 在附件组中的角色
/// </summary>
public enum BlobRoles
{
    Primary = 0,
    Original = 1,
    Thumbnail = 2,
    Snapshot = 3,
    SnapshotThumbnail = 4,
    Gif = 5,
}

/// <summary>
/// Blob 生命周期状态
/// </summary>
public enum BlobStatus
{
    Temp = 0,
    Bound = 1,
    Expired = 2,
}

/// <summary>
/// Blob 消息引用来源类型
/// </summary>
public enum BlobRefSourceTypes
{
    Send = 0,
    Forward = 1,
    History = 2,
}

/// <summary>
/// 物理存储对象生命周期状态
/// </summary>
public enum BlobObjectStatus
{
    Pending = 0,
    Ready = 1,
    Deleting = 2,
}
