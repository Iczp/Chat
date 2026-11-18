using IczpNet.Chat.Blobs;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// Chat ApiHost 层 MinIO 预签名直出 URL 提供器（IczpNet.Chat 核心模块不直接依赖 MinIO SDK）
/// </summary>
public class MinioBlobDirectUrlProvider(IConfiguration configuration) : IBlobDirectUrlProvider
{
    public async Task<string> TryGetPresignedUrlAsync(Blob blob, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(configuration["Blob:Provider"], "Minio", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!configuration.GetValue<bool>("Blob:Minio:EnableDirectRedirect", false))
        {
            return null;
        }

        var endPoint = configuration["Blob:Minio:EndPoint"];
        var accessKey = configuration["Blob:Minio:AccessKey"];
        var secretKey = configuration["Blob:Minio:SecretKey"];
        if (string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            return null;
        }

        var withSsl = configuration.GetValue<bool>("Blob:Minio:WithSSL");
        var client = new MinioClient()
            .WithEndpoint(endPoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(withSsl)
            .Build();

        var expirySeconds = Math.Clamp((int)expiry.TotalSeconds, 60, 7 * 24 * 3600);
        var args = new PresignedGetObjectArgs()
            .WithBucket(blob.Container)
            .WithObject(blob.Name)
            .WithExpiry(expirySeconds);

        return await client.PresignedGetObjectAsync(args);
    }
}
