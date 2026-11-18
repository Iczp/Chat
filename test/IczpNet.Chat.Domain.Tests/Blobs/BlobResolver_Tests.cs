using IczpNet.Chat.Blobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.Blobs;

[DependsOn(typeof(AbpDddDomainModule), typeof(AbpTimingModule))]
public class BlobResolverTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<BlobResolver>();
    }
}

public class BlobResolver_Tests : AbpIntegratedTest<BlobResolverTestModule>
{
    private readonly BlobResolver _blobResolver;

    public BlobResolver_Tests()
    {
        _blobResolver = GetRequiredService<BlobResolver>();
        _blobResolver.LazyServiceProvider = GetRequiredService<IAbpLazyServiceProvider>();
    }

    [Fact]
    public async Task GetDirectoryNameAsync_Should_Use_Date_Hierarchy_Without_SessionIds()
    {
        var sessionId = Guid.NewGuid();
        var sessionUnitId = Guid.NewGuid();
        var dir = await _blobResolver.GetDirectoryNameAsync(new BlobContext
        {
            BlobId = Guid.NewGuid(),
            Container = "chat-files",
            SessionId = sessionId,
            SessionUnitId = sessionUnitId,
            ChatObjectId = 1001,
            MediaType = AttachmentMediaType.Video
        });

        var now = GetRequiredService<IClock>().Now;
        var expected = $"{now:yyyy}/{now:yyyy-MM}/{now:yyyy-MM-dd}/videos";
        Assert.Equal(expected, dir);
        Assert.DoesNotContain(sessionId.ToString(), dir);
        Assert.DoesNotContain(sessionUnitId.ToString(), dir);
    }

    [Fact]
    public async Task GetFileUrlAsync_And_TryParseBlobId_Should_Roundtrip_And_Support_Legacy_Format()
    {
        var blobId = Guid.NewGuid();
        var url = await _blobResolver.GetFileUrlAsync(blobId, "image/png");

        Assert.Equal($"/api/chat/file/{blobId}", url);
        Assert.True(_blobResolver.TryParseBlobId(url, out var parsedNew));
        Assert.Equal(blobId, parsedNew);

        var signedUrl = $"{url}?su={Guid.NewGuid()}&msg=12345&exp=1999999999&sig=abc";
        Assert.True(_blobResolver.TryParseBlobId(signedUrl, out var parsedSigned));
        Assert.Equal(blobId, parsedSigned);

        var legacyUrl = $"/file?id={blobId}&contentType=image/png";
        Assert.True(_blobResolver.TryParseBlobId(legacyUrl, out var parsedLegacy));
        Assert.Equal(blobId, parsedLegacy);

        var absoluteUrl = $"https://chat.example.com/api/chat/file/{blobId}?exp=123";
        Assert.True(_blobResolver.TryParseBlobId(absoluteUrl, out var parsedAbs));
        Assert.Equal(blobId, parsedAbs);

        Assert.False(_blobResolver.TryParseBlobId("/api/chat/file/not-a-guid", out _));
    }
}
