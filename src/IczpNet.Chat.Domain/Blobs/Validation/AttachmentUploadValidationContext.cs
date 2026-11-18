using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs.Validation;

public class AttachmentUploadValidationContext
{
    public required SessionUnitCacheItem SessionUnit { get; init; }

    public required AttachmentMediaType MediaType { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required string Suffix { get; init; }

    public required long FileSize { get; init; }

    public int? Width { get; init; }

    public int? Height { get; init; }

    public int? DurationMs { get; init; }

    public bool IsOriginal { get; init; }

    public bool HasSnapshot { get; init; }

    /// <summary>
    /// 延迟读取文件流（避免验证器多次全量加载到内存）
    /// </summary>
    public required Func<CancellationToken, Task<Stream>> OpenReadStreamAsync { get; init; }

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
}
