using IczpNet.Chat.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Web;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.Blobs;

[DependsOn(typeof(AbpTimingModule))]
public class BlobSignatureTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<AttachmentSignatureOptions>(opt =>
        {
            opt.SigningKey = "Test.Signing.Key.For.UnitTests.2026";
            opt.DefaultExpireSeconds = 300;
        });
        context.Services.AddTransient<BlobSignatureService>();
    }
}

public class BlobSignature_Tests : AbpIntegratedTest<BlobSignatureTestModule>
{
    private readonly BlobSignatureService _signatureService;
    private readonly IClock _clock;

    public BlobSignature_Tests()
    {
        _signatureService = GetRequiredService<BlobSignatureService>();
        _clock = GetRequiredService<IClock>();
    }

    [Fact]
    public void SignedUrl_Should_Validate_And_Reject_Tampering_Or_Expiration()
    {
        var blobId = Guid.NewGuid();
        var suId = Guid.NewGuid();
        var msgId = 9876543210123L;

        var signed = _signatureService.AppendSignature($"/api/chat/file/{blobId}", blobId, suId, msgId, null, 300);
        var uri = new Uri("https://example.com" + signed);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var exp = long.Parse(query["exp"]!);
        var sig = query["sig"]!;

        // 正常校验通过
        Assert.True(_signatureService.ValidateSignature(blobId, suId, msgId, null, exp, sig));

        // 篡改 messageId / sessionUnitId / blobId 均应失败
        Assert.False(_signatureService.ValidateSignature(blobId, suId, msgId + 1, null, exp, sig));
        Assert.False(_signatureService.ValidateSignature(blobId, Guid.NewGuid(), msgId, null, exp, sig));
        Assert.False(_signatureService.ValidateSignature(Guid.NewGuid(), suId, msgId, null, exp, sig));

        // 过期签名应失败
        var expiredExp = new DateTimeOffset(_clock.Now).ToUnixTimeSeconds() - 10;
        Assert.False(_signatureService.ValidateSignature(blobId, suId, msgId, null, expiredExp, sig));
    }

    [Fact]
    public void SigningKey_Validation_Should_Reject_Default_And_Empty_Keys_Unless_Explicitly_Allowed()
    {
        var defaultOpts = new BlobSignatureService(
            Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions()),
            _clock);
        Assert.Throws<InvalidOperationException>(() =>
            defaultOpts.AppendSignature("/api/chat/file/test", Guid.NewGuid(), Guid.NewGuid()));

        var shortOpts = new BlobSignatureService(
            Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions { SigningKey = "short" }),
            _clock);
        Assert.Throws<InvalidOperationException>(() =>
            shortOpts.AppendSignature("/api/chat/file/test", Guid.NewGuid(), Guid.NewGuid()));

        var allowedDefaultOpts = new BlobSignatureService(
            Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions { AllowInsecureDefaultSigningKey = true }),
            _clock);
        Assert.False(string.IsNullOrWhiteSpace(
            allowedDefaultOpts.AppendSignature("/api/chat/file/test", Guid.NewGuid(), Guid.NewGuid())));
    }
}
