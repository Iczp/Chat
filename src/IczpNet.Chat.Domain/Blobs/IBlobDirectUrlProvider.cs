using System;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 对象存储直出 URL 提供器（如 MinIO Presigned GET URL，用于 302 重定向）
/// </summary>
public interface IBlobDirectUrlProvider
{
    Task<string> TryGetPresignedUrlAsync(Blob blob, TimeSpan expiry, CancellationToken cancellationToken = default);
}

public class NullBlobDirectUrlProvider : IBlobDirectUrlProvider
{
    public Task<string> TryGetPresignedUrlAsync(Blob blob, TimeSpan expiry, CancellationToken cancellationToken = default)
        => Task.FromResult<string>(null);
}
