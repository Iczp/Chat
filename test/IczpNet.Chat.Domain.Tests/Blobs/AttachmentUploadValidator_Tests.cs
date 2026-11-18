using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Validation;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.Testing;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.Blobs;

[DependsOn(typeof(AbpDddDomainModule), typeof(AbpTimingModule))]
public class AttachmentUploadValidatorTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<AttachmentUploadOptions>(_ => { });
        context.Services.AddTransient<IAttachmentUploadValidator, AttachmentSizeAndExtensionValidator>();
        context.Services.AddTransient<IAttachmentUploadValidator, AttachmentMagicHeaderValidator>();
        context.Services.AddTransient<IAttachmentUploadValidator, MediaMetadataUploadValidator>();
        context.Services.AddTransient<AttachmentUploadValidatorManager>();
    }
}

public class AttachmentUploadValidator_Tests : AbpIntegratedTest<AttachmentUploadValidatorTestModule>
{
    private readonly AttachmentUploadValidatorManager _manager;

    public AttachmentUploadValidator_Tests()
    {
        _manager = GetRequiredService<AttachmentUploadValidatorManager>();
    }

    private static SessionUnitCacheItem CreateUnit() => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        OwnerId = 1001,
        IsEnabled = true,
        IsInputEnabled = true,
    };

    [Fact]
    public async Task Should_Reject_Blocked_Executable_Extension()
    {
        var bytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            _manager.ValidateAsync(new AttachmentUploadValidationContext
            {
                SessionUnit = CreateUnit(),
                MediaType = AttachmentMediaType.File,
                FileName = "malware.exe",
                ContentType = "application/octet-stream",
                Suffix = ".exe",
                FileSize = bytes.Length,
                OpenReadStreamAsync = _ => Task.FromResult<Stream>(new MemoryStream(bytes)),
            }));

        Assert.Equal(ChatErrorCodes.Blobs.FileTypeNotAllowed, ex.Code);
    }

    [Fact]
    public async Task Should_Reject_Fake_Image_With_Invalid_Magic_Header()
    {
        var fakeBytes = "not-a-real-png"u8.ToArray();
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            _manager.ValidateAsync(new AttachmentUploadValidationContext
            {
                SessionUnit = CreateUnit(),
                MediaType = AttachmentMediaType.Image,
                FileName = "fake.png",
                ContentType = "image/png",
                Suffix = ".png",
                FileSize = fakeBytes.Length,
                OpenReadStreamAsync = _ => Task.FromResult<Stream>(new MemoryStream(fakeBytes)),
            }));

        Assert.Equal(ChatErrorCodes.Blobs.FileHeaderMismatch, ex.Code);
    }

    [Fact]
    public async Task Should_Pass_Valid_Png_Image()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
        await _manager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = CreateUnit(),
            MediaType = AttachmentMediaType.Image,
            FileName = "photo.png",
            ContentType = "image/png",
            Suffix = ".png",
            FileSize = pngHeader.Length,
            OpenReadStreamAsync = _ => Task.FromResult<Stream>(new MemoryStream(pngHeader)),
        });
    }
}
