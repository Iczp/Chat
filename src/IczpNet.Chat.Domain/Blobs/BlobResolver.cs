using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs;

public partial class BlobResolver : DomainService, IBlobResolver
{
    protected string DateDirectoryName
    {
        get
        {
            var now = Clock.Now;
            return $"{now:yyyy}/{now:yyyy-MM}/{now:yyyy-MM-dd}";
        }
    }

    public virtual async Task<string> GetDirectoryNameAsync(BlobContext context)
    {
        await Task.Yield();
        var mediaFolder = ResolveMediaFolder(context);
        return $"{DateDirectoryName}/{mediaFolder}";
    }

    protected virtual string ResolveMediaFolder(BlobContext context)
    {
        if (!string.IsNullOrWhiteSpace(context?.Folder))
        {
            return context.Folder.Trim().Trim('/').ToLowerInvariant();
        }

        return context?.MediaType switch
        {
            AttachmentMediaType.Image => "images",
            AttachmentMediaType.Sound => "audios",
            AttachmentMediaType.Video => "videos",
            AttachmentMediaType.Snapshot => "snapshots",
            _ => "files",
        };
    }

    public virtual async Task<string> GetFileUrlAsync(Guid blobId, string contentType)
    {
        await Task.Yield();
        return $"/api/chat/file/{blobId}";
    }

    public virtual bool TryParseBlobId(string url, out Guid blobId)
    {
        blobId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var pathOrQuery = url.Trim();
        if (Uri.TryCreate(pathOrQuery, UriKind.Absolute, out var absoluteUri))
        {
            pathOrQuery = absoluteUri.PathAndQuery;
        }

        var routeMatch = RouteBlobIdRegex().Match(pathOrQuery);
        if (routeMatch.Success && Guid.TryParse(routeMatch.Groups["id"].Value, out blobId))
        {
            return true;
        }

        var queryMatch = QueryBlobIdRegex().Match(pathOrQuery);
        if (queryMatch.Success && Guid.TryParse(queryMatch.Groups["id"].Value, out blobId))
        {
            return true;
        }

        return false;
    }

    public virtual async Task<string> GenerateFileNameAsync(string suffix)
    {
        await Task.Yield();
        var randomFileName = Path.GetRandomFileName().Replace(".", "");
        var fileName = Clock.Now.ToString("yyyyMMddHHmmss") + "_" + randomFileName + suffix;
        return fileName.ToLower();
    }

    [GeneratedRegex(@"^/(?:api/chat/)?file/(?<id>[0-9a-fA-F-]{32,36})(?:[/?#]|$)", RegexOptions.IgnoreCase)]
    private static partial Regex RouteBlobIdRegex();

    [GeneratedRegex(@"[?&]id=(?<id>[0-9a-fA-F-]{32,36})(?:[&#]|$)", RegexOptions.IgnoreCase)]
    private static partial Regex QueryBlobIdRegex();
}
