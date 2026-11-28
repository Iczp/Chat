using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.Blobs.Validation;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Controllers;
using IczpNet.Chat.Medias;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.ShortIds;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Imaging;
using Volo.Abp.Json;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class AttachmentProcessingReuseHotPath_Tests
{
    private sealed class TestMediaResolver : IMediaResolver
    {
        public int VideoCallCount { get; private set; }
        public int AudioCallCount { get; private set; }
        public VideoProcessOptions LastVideoOptions { get; private set; }
        public Func<byte[], string, VideoProcessOptions, VideoInfo> VideoHandler { get; set; }
        public Func<byte[], string, AudioInfo> AudioHandler { get; set; }

        public Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName)
            => GetVideoInfoAsync(bytes, fileName, new VideoProcessOptions());

        public Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName, VideoProcessOptions options)
        {
            VideoCallCount++;
            LastVideoOptions = options;
            return Task.FromResult(VideoHandler?.Invoke(bytes, fileName, options));
        }

        public Task<AudioInfo> GetAudioInfoAsync(byte[] bytes, string fileName)
        {
            AudioCallCount++;
            return Task.FromResult(AudioHandler?.Invoke(bytes, fileName));
        }
    }

    private static readonly AttachmentProcessingReuseManager RealRecipeHelper = new(
        Substitute.For<Volo.Abp.Domain.Repositories.IRepository<AttachmentProcessingResult, Guid>>(),
        Substitute.For<Volo.Abp.Domain.Repositories.IRepository<AttachmentProcessingWaiter, Guid>>(),
        Substitute.For<Volo.Abp.Domain.Repositories.IRepository<BlobObject, Guid>>(),
        Substitute.For<Volo.Abp.DistributedLocking.IAbpDistributedLock>());

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] bytes)
        => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };

    private static byte[] CreateTinyPngBytes(int width = 320, int height = 240)
    {
        using var img = new MagickImage(MagickColors.CornflowerBlue, (uint)width, (uint)height);
        img.Format = MagickFormat.Png;
        return img.ToByteArray();
    }

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static IAttachmentProcessingReuseManager CreateRecipeMock(
        Func<AttachmentMediaType, BlobRoles, string, string, long, AttachmentProcessingResult> finder = null)
    {
        var mock = Substitute.For<IAttachmentProcessingReuseManager>();
        mock.IsEnabled().Returns(true);
        mock.ComputeSha256(Arg.Any<byte[]>()).Returns(call => RealRecipeHelper.ComputeSha256(call.Arg<byte[]>()));
        mock.ComputeRecipeHash(Arg.Any<string>()).Returns(call => RealRecipeHelper.ComputeRecipeHash(call.Arg<string>()));
        mock.BuildImagePrimaryRecipeHash(Arg.Any<MessageOptions.ImageOptions>())
            .Returns(call => RealRecipeHelper.BuildImagePrimaryRecipeHash(call.Arg<MessageOptions.ImageOptions>()));
        mock.BuildImageThumbnailRecipeHash(Arg.Any<MessageOptions.ImageOptions>())
            .Returns(call => RealRecipeHelper.BuildImageThumbnailRecipeHash(call.Arg<MessageOptions.ImageOptions>()));
        mock.BuildVideoSnapshotRecipeHash(Arg.Any<MessageOptions.VideoOptions>())
            .Returns(call => RealRecipeHelper.BuildVideoSnapshotRecipeHash(call.Arg<MessageOptions.VideoOptions>()));
        mock.BuildVideoSnapshotThumbnailRecipeHash(Arg.Any<MessageOptions.VideoOptions>())
            .Returns(call => RealRecipeHelper.BuildVideoSnapshotThumbnailRecipeHash(call.Arg<MessageOptions.VideoOptions>()));
        mock.BuildVideoClientSnapshotThumbnailRecipeHash(Arg.Any<MessageOptions.VideoOptions>())
            .Returns(call => RealRecipeHelper.BuildVideoClientSnapshotThumbnailRecipeHash(call.Arg<MessageOptions.VideoOptions>()));
        mock.BuildVideoGifRecipeHash(Arg.Any<MessageOptions.VideoOptions>())
            .Returns(call => RealRecipeHelper.BuildVideoGifRecipeHash(call.Arg<MessageOptions.VideoOptions>()));
        mock.BuildSoundProbeRecipeHash().Returns(_ => RealRecipeHelper.BuildSoundProbeRecipeHash());

        mock.FindReadyResultAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<AttachmentMediaType>(),
                Arg.Any<BlobRoles>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                if (finder == null)
                {
                    return Task.FromResult<AttachmentProcessingResult>(null);
                }

                return Task.FromResult(finder(
                    call.ArgAt<AttachmentMediaType>(4),
                    call.ArgAt<BlobRoles>(5),
                    call.ArgAt<string>(6),
                    call.ArgAt<string>(2),
                    call.ArgAt<long>(3)));
            });

        return mock;
    }

    private static (
        AttachmentController Controller,
        List<Blob> CreatedBlobs,
        IImageResizer ImageResizer,
        TestMediaResolver MediaResolver,
        MessageOptions MessageOptions)
        CreateAttachmentController(
            Guid userId,
            SessionUnitCacheItem unit,
            IAttachmentProcessingReuseManager reuseManager,
            bool populateContentSha256OnCreate = true)
    {
        var createdBlobs = new List<Blob>();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 11, 28, 2, 0, 0, DateTimeKind.Utc));

        var msgOptions = new MessageOptions();
        var uploadOptions = new AttachmentUploadOptions { TempBlobExpireHours = 24 };

        var suMgr = Substitute.For<ISessionUnitManager>();
        suMgr.GetCacheAsync(unit.Id).Returns(unit);

        var chatObjectMgr = Substitute.For<IChatObjectManager>();
        chatObjectMgr.GetIdListByUserIdAsync(userId).Returns(new List<long> { unit.OwnerId });

        var validatorMgr = Substitute.For<IAttachmentUploadValidatorManager>();
        validatorMgr.ValidateAsync(Arg.Any<AttachmentUploadValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mediaResolver = new TestMediaResolver();
        var imageResizer = Substitute.For<IImageResizer>();

        var blobManager = Substitute.For<IBlobManager>();
        blobManager.CreateAsync(Arg.Any<Blob>()).Returns(call =>
        {
            var b = call.Arg<Blob>();
            if (populateContentSha256OnCreate && string.IsNullOrWhiteSpace(b.ContentSha256) && b.Bytes is { Length: > 0 })
            {
                b.ContentSha256 = Sha256Hex(b.Bytes);
            }
            if (!b.StorageObjectId.HasValue)
            {
                b.StorageObjectId = Guid.NewGuid();
            }
            createdBlobs.Add(b);
            return Task.FromResult(b);
        });

        var blobResolver = Substitute.For<IBlobResolver>();
        blobResolver.GetDirectoryNameAsync(Arg.Any<BlobContext>()).Returns("2026/11/media");
        blobResolver.GenerateFileNameAsync(Arg.Any<string>()).Returns(call => $"f_{Guid.NewGuid():N}{call.Arg<string>()}");
        blobResolver.GetFileUrlAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(call => $"/api/chat/file/{call.Arg<Guid>()}");

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);

        var guidGen = Substitute.For<IGuidGenerator>();
        guidGen.Create().Returns(_ => Guid.NewGuid());

        var jsonSerializer = Substitute.For<IJsonSerializer>();
        jsonSerializer.Serialize(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(call => System.Text.Json.JsonSerializer.Serialize(call.Arg<object>()));

        var sigOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions
        {
            SigningKey = "HotPath.Test.Signing.Key.2026",
            DefaultExpireSeconds = 900,
        });
        var tokenService = new AttachmentReuseTokenService(
            sigOptions,
            Microsoft.Extensions.Options.Options.Create(uploadOptions),
            clock);

        var controller = new AttachmentController(
            suMgr,
            chatObjectMgr,
            validatorMgr,
            mediaResolver,
            Microsoft.Extensions.Options.Options.Create(msgOptions),
            Microsoft.Extensions.Options.Options.Create(uploadOptions),
            jsonSerializer);

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
        lazy.LazyGetRequiredService<ICurrentTenant>().Returns(currentTenant);
        lazy.LazyGetService<ICurrentTenant>().Returns(currentTenant);
        lazy.LazyGetRequiredService<IBlobManager>().Returns(blobManager);
        lazy.LazyGetRequiredService<IBlobResolver>().Returns(blobResolver);
        lazy.LazyGetRequiredService<IImageResizer>().Returns(imageResizer);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(guidGen);
        lazy.LazyGetService<IGuidGenerator>().Returns(guidGen);
        lazy.LazyGetService<IGuidGenerator>(Arg.Any<IGuidGenerator>()).Returns(guidGen);
        lazy.LazyGetRequiredService<IAttachmentProcessingReuseManager>().Returns(reuseManager);
        lazy.LazyGetRequiredService<IAttachmentReuseTokenService>().Returns(tokenService);
        controller.LazyServiceProvider = lazy;

        return (controller, createdBlobs, imageResizer, mediaResolver, msgOptions);
    }

    private static (
        MessageSenderController Controller,
        List<Blob> CreatedBlobs,
        IImageResizer ImageResizer,
        TestMediaResolver MediaResolver,
        IMessageSenderAppService SenderAppService,
        MessageOptions MessageOptions)
        CreateMessageSenderController(
            Guid userId,
            SessionUnitCacheItem unit,
            IAttachmentProcessingReuseManager reuseManager)
    {
        var createdBlobs = new List<Blob>();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 11, 28, 2, 0, 0, DateTimeKind.Utc));

        var msgOptions = new MessageOptions();
        var uploadOptions = new AttachmentUploadOptions { TempBlobExpireHours = 24 };

        var suMgr = Substitute.For<ISessionUnitManager>();
        suMgr.GetCacheAsync(unit.Id).Returns(unit);

        var chatObjectMgr = Substitute.For<IChatObjectManager>();
        chatObjectMgr.GetIdListByUserIdAsync(userId).Returns(new List<long> { unit.OwnerId });

        var validatorMgr = Substitute.For<IAttachmentUploadValidatorManager>();
        validatorMgr.ValidateAsync(Arg.Any<AttachmentUploadValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var senderAppService = Substitute.For<IMessageSenderAppService>();
        senderAppService.SendImageAsync(Arg.Any<Guid>(), Arg.Any<MessageInput<ImageContentInfo>>())
            .Returns(call => new MessageInfo<ImageContentInfo> { Id = 2001L, Content = call.Arg<MessageInput<ImageContentInfo>>().Content });
        senderAppService.SendVideoAsync(Arg.Any<Guid>(), Arg.Any<MessageInput<VideoContentInfo>>())
            .Returns(call => new MessageInfo<VideoContentInfo> { Id = 2002L, Content = call.Arg<MessageInput<VideoContentInfo>>().Content });
        senderAppService.SendSoundAsync(Arg.Any<Guid>(), Arg.Any<MessageInput<SoundContentInfo>>())
            .Returns(call => new MessageInfo<SoundContentInfo> { Id = 2003L, Content = call.Arg<MessageInput<SoundContentInfo>>().Content });

        var mediaResolver = new TestMediaResolver();
        var imageResizer = Substitute.For<IImageResizer>();

        var blobManager = Substitute.For<IBlobManager>();
        blobManager.CreateAsync(Arg.Any<Blob>()).Returns(call =>
        {
            var b = call.Arg<Blob>();
            if (string.IsNullOrWhiteSpace(b.ContentSha256) && b.Bytes is { Length: > 0 })
            {
                b.ContentSha256 = Sha256Hex(b.Bytes);
            }
            if (!b.StorageObjectId.HasValue)
            {
                b.StorageObjectId = Guid.NewGuid();
            }
            createdBlobs.Add(b);
            return Task.FromResult(b);
        });

        var blobResolver = Substitute.For<IBlobResolver>();
        blobResolver.GetDirectoryNameAsync(Arg.Any<BlobContext>()).Returns("2026/11/media");
        blobResolver.GenerateFileNameAsync(Arg.Any<string>()).Returns(call => $"f_{Guid.NewGuid():N}{call.Arg<string>()}");
        blobResolver.GetFileUrlAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(call => $"/api/chat/file/{call.Arg<Guid>()}");

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);

        var guidGen = Substitute.For<IGuidGenerator>();
        guidGen.Create().Returns(_ => Guid.NewGuid());
        var shortIdGen = Substitute.For<IShortIdGenerator>();
        shortIdGen.Create().Returns("hot12345");

        var jsonSerializer = Substitute.For<IJsonSerializer>();
        jsonSerializer.Serialize(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(call => System.Text.Json.JsonSerializer.Serialize(call.Arg<object>()));

        var controller = new MessageSenderController(
            suMgr,
            senderAppService,
            Microsoft.Extensions.Options.Options.Create(msgOptions),
            jsonSerializer,
            mediaResolver);

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
        lazy.LazyGetRequiredService<ICurrentTenant>().Returns(currentTenant);
        lazy.LazyGetService<ICurrentTenant>().Returns(currentTenant);
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(chatObjectMgr);
        lazy.LazyGetRequiredService<Microsoft.Extensions.Options.IOptions<AttachmentUploadOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(uploadOptions));
        lazy.LazyGetRequiredService<IAttachmentUploadValidatorManager>().Returns(validatorMgr);
        lazy.LazyGetRequiredService<IBlobManager>().Returns(blobManager);
        lazy.LazyGetRequiredService<IBlobResolver>().Returns(blobResolver);
        lazy.LazyGetRequiredService<IImageResizer>().Returns(imageResizer);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(guidGen);
        lazy.LazyGetService<IGuidGenerator>().Returns(guidGen);
        lazy.LazyGetService<IGuidGenerator>(Arg.Any<IGuidGenerator>()).Returns(guidGen);
        lazy.LazyGetRequiredService<IShortIdGenerator>().Returns(shortIdGen);
        lazy.LazyGetService<IAttachmentProcessingReuseManager>().Returns(reuseManager);
        controller.LazyServiceProvider = lazy;

        return (controller, createdBlobs, imageResizer, mediaResolver, senderAppService, msgOptions);
    }

    [Fact]
    public async Task UploadImageAsync_skips_ImageMagick_decode_and_resize_when_primary_and_thumbnail_cache_hit_with_metadata()
    {
        var userId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 101L, SessionId = Guid.NewGuid(), IsEnabled = true, IsInputEnabled = true };
        // 故意传入非合法图片字节：若代码仍急切调用 new MagickImage(sourceBytes) 将直接抛异常
        var dummyBytes = Encoding.UTF8.GetBytes("cached-image-source-not-decodable-by-magick");
        var sha = Sha256Hex(dummyBytes);
        var opts = new MessageOptions();
        var primaryRecipe = RealRecipeHelper.BuildImagePrimaryRecipeHash(opts.ImageSetting);
        var thumbRecipe = RealRecipeHelper.BuildImageThumbnailRecipeHash(opts.ImageSetting);
        var primaryStorageId = Guid.NewGuid();
        var thumbStorageId = Guid.NewGuid();

        var reuseMgr = CreateRecipeMock((mediaType, role, recipe, srcSha, srcSize) =>
        {
            if (mediaType == AttachmentMediaType.Image && role == BlobRoles.Primary && recipe == primaryRecipe && srcSha == sha && srcSize == dummyBytes.LongLength)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, dummyBytes.LongLength, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = primaryStorageId,
                    OutputMimeType = "image/jpeg",
                    OutputSuffix = ".jpg",
                    OutputFileSize = 54321,
                    Width = 1080,
                    Height = 720,
                    MetadataJson = "[]",
                };
            }

            if (mediaType == AttachmentMediaType.Image && role == BlobRoles.Thumbnail && recipe == thumbRecipe && srcSha == sha && srcSize == dummyBytes.LongLength)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, dummyBytes.LongLength, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = thumbStorageId,
                    OutputMimeType = "image/jpeg",
                    OutputSuffix = ".jpg",
                    OutputFileSize = 3210,
                };
            }

            return null;
        });

        var (controller, blobs, imageResizer, _, _) = CreateAttachmentController(userId, unit, reuseMgr);
        var output = await controller.UploadImageAsync(unit.Id, CreateFormFile("photo.jpg", "image/jpeg", dummyBytes));

        Assert.Equal(1080, output.Width);
        Assert.Equal(720, output.Height);
        Assert.Equal("[]", output.Profile);
        Assert.Equal(54321, output.Size);
        Assert.NotNull(output.ThumbnailBlobId);
        Assert.Equal(2, blobs.Count);
        Assert.All(blobs, b => Assert.Equal(BlobStatus.Temp, b.Status));
        await imageResizer.DidNotReceive().ResizeAsync(Arg.Any<byte[]>(), Arg.Any<ImageResizeArgs>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendUploadImageAsync_cache_hit_does_not_crash_on_null_blob_bytes_and_shares_recipe_with_AttachmentController()
    {
        var userId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 101L, SessionId = Guid.NewGuid(), IsEnabled = true, IsInputEnabled = true };
        // 非合法图片字节 + 命中缓存返回 blob.Bytes == null，同时验证免解码和不崩溃
        var dummyBytes = Encoding.UTF8.GetBytes("legacy-send-upload-image-cache-hit");
        var sha = Sha256Hex(dummyBytes);
        var opts = new MessageOptions();
        var primaryRecipe = RealRecipeHelper.BuildImagePrimaryRecipeHash(opts.ImageSetting);
        var thumbRecipe = RealRecipeHelper.BuildImageThumbnailRecipeHash(opts.ImageSetting);
        var primaryStorageId = Guid.NewGuid();
        var thumbStorageId = Guid.NewGuid();

        var reuseMgr = CreateRecipeMock((mediaType, role, recipe, srcSha, srcSize) =>
        {
            if (mediaType == AttachmentMediaType.Image && role == BlobRoles.Primary && recipe == primaryRecipe && srcSha == sha && srcSize == dummyBytes.LongLength)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, dummyBytes.LongLength, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = primaryStorageId,
                    OutputMimeType = "image/png",
                    OutputSuffix = ".png",
                    OutputFileSize = 66666,
                    Width = 900,
                    Height = 600,
                    MetadataJson = "[]",
                };
            }

            if (mediaType == AttachmentMediaType.Image && role == BlobRoles.Thumbnail && recipe == thumbRecipe && srcSha == sha && srcSize == dummyBytes.LongLength)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, dummyBytes.LongLength, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = thumbStorageId,
                    OutputMimeType = "image/png",
                    OutputSuffix = ".png",
                    OutputFileSize = 2222,
                };
            }

            return null;
        });

        var (controller, blobs, imageResizer, _, senderAppService, _) = CreateMessageSenderController(userId, unit, reuseMgr);
        var actionResult = await controller.SendUploadImageAsync(unit.Id, CreateFormFile("pic.png", "image/png", dummyBytes), 0, [], isOriginal: false);

        var jsonResult = Assert.IsType<JsonResult>(actionResult);
        var msgInfo = Assert.IsType<MessageInfo<ImageContentInfo>>(jsonResult.Value);
        Assert.Equal(900, msgInfo.Content.Width);
        Assert.Equal(600, msgInfo.Content.Height);
        Assert.Equal(66666, msgInfo.Content.Size);
        Assert.Equal("[]", msgInfo.Content.Profile);
        Assert.NotNull(msgInfo.Content.BlobId);
        Assert.NotNull(msgInfo.Content.ThumbnailBlobId);
        Assert.Equal(2, blobs.Count);
        Assert.All(blobs, b => Assert.Equal(BlobStatus.Temp, b.Status));
        Assert.All(blobs, b => Assert.Null(b.Bytes));
        await imageResizer.DidNotReceive().ResizeAsync(Arg.Any<byte[]>(), Arg.Any<ImageResizeArgs>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await senderAppService.Received(1).SendImageAsync(unit.Id, Arg.Any<MessageInput<ImageContentInfo>>());
    }

    [Fact]
    public async Task SendUploadVideoAsync_cache_hit_sets_SnapshotBlobId_Thumbnail_and_Gif_with_Temp_status_and_skips_FFmpeg()
    {
        var userId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 101L, SessionId = Guid.NewGuid(), IsEnabled = true, IsInputEnabled = true };
        var videoBytes = Encoding.UTF8.GetBytes("cached-video-bytes-for-legacy-endpoint");
        var sha = Sha256Hex(videoBytes);
        var opts = new MessageOptions();
        var snapRecipe = RealRecipeHelper.BuildVideoSnapshotRecipeHash(opts.VideoSetting);
        var thumbRecipe = RealRecipeHelper.BuildVideoSnapshotThumbnailRecipeHash(opts.VideoSetting);
        var gifRecipe = RealRecipeHelper.BuildVideoGifRecipeHash(opts.VideoSetting);

        var snapStorageId = Guid.NewGuid();
        var thumbStorageId = Guid.NewGuid();
        var gifStorageId = Guid.NewGuid();

        var reuseMgr = CreateRecipeMock((mediaType, role, recipe, srcSha, srcSize) =>
        {
            if (mediaType != AttachmentMediaType.Video || srcSha != sha || srcSize != videoBytes.LongLength)
            {
                return null;
            }

            if (role == BlobRoles.Snapshot && recipe == snapRecipe)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, srcSize, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = snapStorageId,
                    OutputMimeType = "image/png",
                    OutputSuffix = ".png",
                    OutputFileSize = 11111,
                    Width = 1280,
                    Height = 720,
                    DurationMs = 15200,
                };
            }

            if (role == BlobRoles.SnapshotThumbnail && recipe == thumbRecipe)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, srcSize, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = thumbStorageId,
                    OutputMimeType = "image/png",
                    OutputSuffix = ".png",
                    OutputFileSize = 2222,
                };
            }

            if (role == BlobRoles.Gif && recipe == gifRecipe)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, srcSize, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = gifStorageId,
                    OutputMimeType = "image/gif",
                    OutputSuffix = ".gif",
                    OutputFileSize = 33333,
                };
            }

            return null;
        });

        var (controller, blobs, _, mediaResolver, _, _) = CreateMessageSenderController(userId, unit, reuseMgr);
        var actionResult = await controller.SendUploadVideoAsync(unit.Id, CreateFormFile("clip.mp4", "video/mp4", videoBytes), 0, [], isOriginal: false);

        var jsonResult = Assert.IsType<JsonResult>(actionResult);
        var msgInfo = Assert.IsType<MessageInfo<VideoContentInfo>>(jsonResult.Value);
        Assert.NotNull(msgInfo.Content.BlobId);
        Assert.NotNull(msgInfo.Content.SnapshotBlobId);
        Assert.NotNull(msgInfo.Content.SnapshotUrl);
        Assert.NotNull(msgInfo.Content.SnapshotThumbnailBlobId);
        Assert.NotNull(msgInfo.Content.SnapshotThumbnailUrl);
        Assert.NotNull(msgInfo.Content.GifBlobId);
        Assert.NotNull(msgInfo.Content.GifUrl);
        Assert.Equal(1280, msgInfo.Content.Width);
        Assert.Equal(720, msgInfo.Content.Height);
        Assert.Equal(15.2, msgInfo.Content.Duration);

        // 4 个逻辑 Blob（主视频 + 封面 + 封面缩略图 + GIF）全部为 Temp，且子 Blob 正确指向主视频
        Assert.Equal(4, blobs.Count);
        Assert.All(blobs, b => Assert.Equal(BlobStatus.Temp, b.Status));
        Assert.Equal(3, blobs.Count(b => b.ParentBlobId == msgInfo.Content.BlobId));
        Assert.Equal(0, mediaResolver.VideoCallCount);
    }

    [Fact]
    public async Task UploadVideoAsync_without_snapshot_never_generates_gif_synchronously_and_avoids_double_sha256()
    {
        var userId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 101L, SessionId = Guid.NewGuid(), IsEnabled = true, IsInputEnabled = true };
        var videoBytes = Encoding.UTF8.GetBytes("cold-video-bytes-without-client-snapshot");

        // 场景 A：封面与缩略图已命中但 GIF 尚未生成 -> 不应触发同步 FFmpeg，且因 Blob.ContentSha256 已填充不再二次哈希
        var sha = Sha256Hex(videoBytes);
        var opts = new MessageOptions();
        var snapRecipe = RealRecipeHelper.BuildVideoSnapshotRecipeHash(opts.VideoSetting);
        var thumbRecipe = RealRecipeHelper.BuildVideoSnapshotThumbnailRecipeHash(opts.VideoSetting);

        var hitReuseMgr = CreateRecipeMock((mediaType, role, recipe, srcSha, srcSize) =>
        {
            if (mediaType == AttachmentMediaType.Video && srcSha == sha && srcSize == videoBytes.LongLength)
            {
                if (role == BlobRoles.Snapshot && recipe == snapRecipe)
                {
                    return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, srcSize, mediaType, role, recipe)
                    {
                        Status = AttachmentProcessingStatus.Ready,
                        OutputStorageObjectId = Guid.NewGuid(),
                        OutputMimeType = "image/png",
                        OutputSuffix = ".png",
                        OutputFileSize = 4096,
                        Width = 640,
                        Height = 360,
                        DurationMs = 8000,
                    };
                }
                if (role == BlobRoles.SnapshotThumbnail && recipe == thumbRecipe)
                {
                    return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", sha, srcSize, mediaType, role, recipe)
                    {
                        Status = AttachmentProcessingStatus.Ready,
                        OutputStorageObjectId = Guid.NewGuid(),
                        OutputMimeType = "image/png",
                        OutputSuffix = ".png",
                        OutputFileSize = 1024,
                    };
                }
            }
            return null;
        });

        var (hitController, _, _, hitMediaResolver, _) = CreateAttachmentController(userId, unit, hitReuseMgr, populateContentSha256OnCreate: true);
        var hitOutput = await hitController.UploadVideoAsync(unit.Id, CreateFormFile("clip.mp4", "video/mp4", videoBytes));

        Assert.NotNull(hitOutput.SnapshotBlobId);
        Assert.NotNull(hitOutput.SnapshotThumbnailBlobId);
        Assert.Null(hitOutput.GifBlobId);
        Assert.Equal(0, hitMediaResolver.VideoCallCount);
        hitReuseMgr.DidNotReceive().ComputeSha256(Arg.Any<byte[]>());

        // 场景 B：冷视频未命中缓存 -> 必须以 IncludeSnapshot=true, IncludeGif=false 调用 GetVideoInfoAsync
        var missReuseMgr = CreateRecipeMock();
        var (missController, _, _, missMediaResolver, _) = CreateAttachmentController(userId, unit, missReuseMgr, populateContentSha256OnCreate: true);

        await missController.UploadVideoAsync(unit.Id, CreateFormFile("cold.mp4", "video/mp4", videoBytes));

        Assert.Equal(1, missMediaResolver.VideoCallCount);
        Assert.NotNull(missMediaResolver.LastVideoOptions);
        Assert.True(missMediaResolver.LastVideoOptions.IncludeSnapshot);
        Assert.False(missMediaResolver.LastVideoOptions.IncludeGif);
        missReuseMgr.DidNotReceive().ComputeSha256(Arg.Any<byte[]>());
    }

    [Fact]
    public async Task UploadVideoAsync_with_client_snapshot_reuses_snapshot_thumbnail_from_processing_result()
    {
        var userId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 101L, SessionId = Guid.NewGuid(), IsEnabled = true, IsInputEnabled = true };
        var videoBytes = Encoding.UTF8.GetBytes("video-with-client-snapshot-payload");
        var snapBytes = CreateTinyPngBytes(400, 300);
        var snapSha = Sha256Hex(snapBytes);
        var opts = new MessageOptions();
        var clientThumbRecipe = RealRecipeHelper.BuildVideoClientSnapshotThumbnailRecipeHash(opts.VideoSetting);
        var reusedThumbStorageId = Guid.NewGuid();

        var reuseMgr = CreateRecipeMock((mediaType, role, recipe, srcSha, srcSize) =>
        {
            if (mediaType == AttachmentMediaType.Snapshot &&
                role == BlobRoles.SnapshotThumbnail &&
                recipe == clientThumbRecipe &&
                srcSha == snapSha &&
                srcSize == snapBytes.LongLength)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", snapSha, srcSize, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = reusedThumbStorageId,
                    OutputMimeType = "image/png",
                    OutputSuffix = ".png",
                    OutputFileSize = 888,
                };
            }

            return null;
        });

        var (controller, blobs, imageResizer, _, _) = CreateAttachmentController(userId, unit, reuseMgr, populateContentSha256OnCreate: true);
        var output = await controller.UploadVideoAsync(
            unit.Id,
            CreateFormFile("v.mp4", "video/mp4", videoBytes),
            snapshotFile: CreateFormFile("cover.png", "image/png", snapBytes));

        Assert.NotNull(output.SnapshotBlobId);
        Assert.NotNull(output.SnapshotThumbnailBlobId);
        var thumbBlob = blobs.Single(b => b.Id == output.SnapshotThumbnailBlobId);
        Assert.Equal(reusedThumbStorageId, thumbBlob.StorageObjectId);
        await imageResizer.DidNotReceive().ResizeAsync(Arg.Any<byte[]>(), Arg.Any<ImageResizeArgs>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        reuseMgr.DidNotReceive().ComputeSha256(Arg.Any<byte[]>());
    }

    [Fact]
    public async Task UploadSoundAsync_and_SendUploadSoundAsync_share_sound_probe_recipe_and_guard_empty_duration()
    {
        var userId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 101L, SessionId = Guid.NewGuid(), IsEnabled = true, IsInputEnabled = true };
        var soundBytes = Encoding.UTF8.GetBytes("voice-payload-bytes");
        var soundSha = Sha256Hex(soundBytes);
        var expectedRecipe = RealRecipeHelper.BuildSoundProbeRecipeHash();

        var reuseMgr = CreateRecipeMock((mediaType, role, recipe, srcSha, srcSize) =>
        {
            if (mediaType == AttachmentMediaType.Sound &&
                role == BlobRoles.Primary &&
                recipe == expectedRecipe &&
                srcSha == soundSha &&
                srcSize == soundBytes.LongLength)
            {
                return new AttachmentProcessingResult(Guid.NewGuid(), "host", "chat-files", soundSha, srcSize, mediaType, role, recipe)
                {
                    Status = AttachmentProcessingStatus.Ready,
                    OutputStorageObjectId = Guid.NewGuid(),
                    OutputMimeType = "audio/mpeg",
                    OutputSuffix = ".mp3",
                    OutputFileSize = soundBytes.LongLength,
                    DurationMs = 4321,
                };
            }

            return null;
        });

        var (attController, _, _, attMediaResolver, _) = CreateAttachmentController(userId, unit, reuseMgr);
        var uploadResult = await attController.UploadSoundAsync(unit.Id, CreateFormFile("v.mp3", "audio/mpeg", soundBytes));
        Assert.Equal(4321, uploadResult.Duration);
        Assert.Equal(0, attMediaResolver.AudioCallCount);

        var (legacyController, _, _, legacyMediaResolver, _, _) = CreateMessageSenderController(userId, unit, reuseMgr);
        var legacyAction = await legacyController.SendUploadSoundAsync(unit.Id, CreateFormFile("v.mp3", "audio/mpeg", soundBytes), 0, [], duration: null);
        var legacyMsg = Assert.IsType<MessageInfo<SoundContentInfo>>(Assert.IsType<JsonResult>(legacyAction).Value);
        Assert.Equal(4321, legacyMsg.Content.Time);
        Assert.Equal(0, legacyMediaResolver.AudioCallCount);

        // 未命中且 MediaResolver 返回 Duration == null 时不应抛 InvalidOperationException
        var missReuseMgr = CreateRecipeMock();
        var (missLegacyController, _, _, missMediaResolver, _, _) = CreateMessageSenderController(userId, unit, missReuseMgr);
        missMediaResolver.AudioHandler = (_, _) => new AudioInfo { Duration = null };
        var safeAction = await missLegacyController.SendUploadSoundAsync(unit.Id, CreateFormFile("v.mp3", "audio/mpeg", soundBytes), 0, [], duration: null);
        var safeMsg = Assert.IsType<MessageInfo<SoundContentInfo>>(Assert.IsType<JsonResult>(safeAction).Value);
        Assert.Equal(0, safeMsg.Content.Time);
        Assert.Equal(1, missMediaResolver.AudioCallCount);
    }
}
