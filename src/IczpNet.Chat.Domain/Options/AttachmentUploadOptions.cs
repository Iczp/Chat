using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Options;

public class AttachmentUploadOptions
{
    /// <summary>
    /// 临时未绑定消息的附件过期时间（小时）
    /// </summary>
    public int TempBlobExpireHours { get; set; } = 24;

    /// <summary>
    /// 单图片最大字节数（默认 20 MB）
    /// </summary>
    public long MaxImageSize { get; set; } = 20L * 1024 * 1024;

    /// <summary>
    /// 单视频最大字节数（默认 200 MB）
    /// </summary>
    public long MaxVideoSize { get; set; } = 200L * 1024 * 1024;

    /// <summary>
    /// 单语音最大字节数（默认 20 MB）
    /// </summary>
    public long MaxSoundSize { get; set; } = 20L * 1024 * 1024;

    /// <summary>
    /// 普通文件最大字节数（默认 100 MB）
    /// </summary>
    public long MaxFileSize { get; set; } = 100L * 1024 * 1024;

    /// <summary>
    /// 封面图/缩略图最大字节数（默认 5 MB）
    /// </summary>
    public long MaxSnapshotSize { get; set; } = 5L * 1024 * 1024;

    /// <summary>
    /// 禁止上传的危险扩展名黑名单
    /// </summary>
    public HashSet<string> BlockedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".vbs", ".js", ".msi", ".scr", ".com", ".pif"
    };

    /// <summary>
    /// 允许的图片扩展名
    /// </summary>
    public HashSet<string> AllowedImageExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    /// <summary>
    /// 允许的视频扩展名
    /// </summary>
    public HashSet<string> AllowedVideoExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".m4v", ".webm", ".avi", ".mkv"
    };

    /// <summary>
    /// 允许的语音扩展名
    /// </summary>
    public HashSet<string> AllowedSoundExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".aac", ".ogg", ".opus", ".amr", ".webm"
    };

    /// <summary>
    /// 视频上传时是否强制要求前端提供封面图
    /// </summary>
    public bool RequireVideoSnapshotOnUpload { get; set; } = false;

    /// <summary>
    /// 是否启用文件头魔数验证
    /// </summary>
    public bool EnableMagicHeaderValidation { get; set; } = true;

    /// <summary>
    /// 是否启用服务端物理存储去重（关闭后新上传回退到按逻辑路径直存，旧共享对象仍兼容读取与清理）
    /// </summary>
    public bool EnablePhysicalDeduplication { get; set; } = true;
}
