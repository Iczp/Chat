using IczpNet.Chat.Blobs;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Json;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class MessageSenderUploadAuthorization_Tests
{
    private static (MessageSenderController Controller, List<Blob> CreatedBlobs, IMessageSenderAppService SenderAppService) CreateController(
        Guid? userId,
        long[] ownedChatObjects,
        SessionUnitCacheItem sessionUnit)
    {
        var createdBlobs = new List<Blob>();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 11, 27, 8, 0, 0, DateTimeKind.Utc));

        var suMgr = Substitute.For<ISessionUnitManager>();
        suMgr.GetCacheAsync(sessionUnit.Id).Returns(sessionUnit);

        var senderAppService = Substitute.For<IMessageSenderAppService>();
        senderAppService.SendFileAsync(Arg.Any<Guid>(), Arg.Any<MessageInput<FileContentInfo>>())
            .Returns(call => new MessageInfo<FileContentInfo> { Id = 1001L, Content = call.Arg<MessageInput<FileContentInfo>>().Content });
        senderAppService.SendSoundAsync(Arg.Any<Guid>(), Arg.Any<MessageInput<SoundContentInfo>>())
            .Returns(call => new MessageInfo<SoundContentInfo> { Id = 1002L, Content = call.Arg<MessageInput<SoundContentInfo>>().Content });

        var msgOptions = Microsoft.Extensions.Options.Options.Create(new MessageOptions());
        var uploadOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentUploadOptions { TempBlobExpireHours = 12 });
        var jsonSerializer = Substitute.For<IJsonSerializer>();
        var mediaResolver = Substitute.For<IMediaResolver>();
        mediaResolver.GetAudioInfoAsync(Arg.Any<byte[]>(), Arg.Any<string>()).Returns((AudioInfo)null);

        var controller = new MessageSenderController(suMgr, senderAppService, msgOptions, jsonSerializer, mediaResolver);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        var chatObjectManager = Substitute.For<IChatObjectManager>();
        if (userId.HasValue)
        {
            chatObjectManager.GetIdListByUserIdAsync(userId.Value).Returns(ownedChatObjects.ToList());
        }

        var blobManager = Substitute.For<IBlobManager>();
        blobManager.CreateAsync(Arg.Any<Blob>()).Returns(call =>
        {
            var blob = call.Arg<Blob>();
            createdBlobs.Add(blob);
            return Task.FromResult(blob);
        });

        var blobResolver = Substitute.For<IBlobResolver>();
        blobResolver.GetDirectoryNameAsync(Arg.Any<BlobContext>()).Returns("2026/11/files");
        blobResolver.GenerateFileNameAsync(Arg.Any<string>()).Returns(call => $"f_{Guid.NewGuid():N}{call.Arg<string>()}");
        blobResolver.GetFileUrlAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(call => $"/api/chat/file/{call.Arg<Guid>()}");

        var guidGen = Substitute.For<IGuidGenerator>();
        guidGen.Create().Returns(_ => Guid.NewGuid());
        var shortIdGen = Substitute.For<IShortIdGenerator>();
        shortIdGen.Create().Returns("abc12345");

        var validatorManager = Substitute.For<IAttachmentUploadValidatorManager>();
        validatorManager.ValidateAsync(Arg.Any<AttachmentUploadValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(chatObjectManager);
        lazy.LazyGetRequiredService<Microsoft.Extensions.Options.IOptions<AttachmentUploadOptions>>().Returns(uploadOptions);
        lazy.LazyGetRequiredService<IAttachmentUploadValidatorManager>().Returns(validatorManager);
        lazy.LazyGetRequiredService<IBlobManager>().Returns(blobManager);
        lazy.LazyGetRequiredService<IBlobResolver>().Returns(blobResolver);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(guidGen);
        lazy.LazyGetService<IGuidGenerator>().Returns(guidGen);
        lazy.LazyGetService<IGuidGenerator>(Arg.Any<IGuidGenerator>()).Returns(guidGen);
        lazy.LazyGetRequiredService<IShortIdGenerator>().Returns(shortIdGen);
        controller.LazyServiceProvider = lazy;

        return (controller, createdBlobs, senderAppService);
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] bytes)
        => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

    [Fact]
    public async Task SendUploadFile_and_Sound_create_private_owned_Temp_blobs_and_enforce_sessionUnit_ownership()
    {
        var userId = Guid.NewGuid();
        var suId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = suId, OwnerId = 101L, SessionId = sessionId, IsEnabled = true, IsInputEnabled = true };

        // 1. Owned user succeeds and creates private Temp Primary blob
        var (ctrl, blobs, _) = CreateController(userId, [101L], unit);
        var fileResult = await ctrl.SendUploadFileAsync(suId, CreateFormFile("doc.pdf", "application/pdf", Encoding.UTF8.GetBytes("hello")), 0, []);
        Assert.IsType<JsonResult>(fileResult);
        var fileBlob = Assert.Single(blobs);
        Assert.False(fileBlob.IsPublic);
        Assert.Equal(BlobStatus.Temp, fileBlob.Status);
        Assert.Equal(BlobRoles.Primary, fileBlob.Role);
        Assert.Equal(101L, fileBlob.UploaderChatObjectId);
        Assert.Equal(suId, fileBlob.UploadSessionUnitId);
        Assert.Equal(sessionId, fileBlob.UploadSessionId);

        var soundResult = await ctrl.SendUploadSoundAsync(suId, CreateFormFile("voice.mp3", "audio/mpeg", [1, 2, 3, 4]), 0, [], 1500);
        Assert.IsType<JsonResult>(soundResult);
        Assert.Equal(2, blobs.Count);
        Assert.False(blobs[1].IsPublic);
        Assert.Equal(BlobStatus.Temp, blobs[1].Status);
        Assert.Equal(101L, blobs[1].UploaderChatObjectId);

        // 2. Non-owner user rejected
        var (otherCtrl, otherBlobs, _) = CreateController(Guid.NewGuid(), [202L], unit);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            otherCtrl.SendUploadFileAsync(suId, CreateFormFile("doc.pdf", "application/pdf", [1, 2, 3]), 0, []));
        Assert.Empty(otherBlobs);

        // 3. Anonymous rejected
        var (anonCtrl, _, _) = CreateController(null, [], unit);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            anonCtrl.SendUploadFileAsync(suId, CreateFormFile("doc.pdf", "application/pdf", [1, 2, 3]), 0, []));

        // 4. Disabled SessionUnit rejected
        var disabledUnit = new SessionUnitCacheItem { Id = suId, OwnerId = 101L, SessionId = sessionId, IsEnabled = true, IsInputEnabled = false };
        var (disabledCtrl, _, _) = CreateController(userId, [101L], disabledUnit);
        await Assert.ThrowsAsync<BusinessException>(() =>
            disabledCtrl.SendUploadFileAsync(suId, CreateFormFile("doc.pdf", "application/pdf", [1, 2, 3]), 0, []));

        // 5. Blocked extension rejected via real AttachmentSizeAndExtensionValidator
        var blockedCtrl = CreateController(userId, [101L], unit).Controller;
        var realValidator = new AttachmentUploadValidatorManager(
            [new AttachmentSizeAndExtensionValidator(Microsoft.Extensions.Options.Options.Create(new AttachmentUploadOptions()))]);
        var lazyProvider = blockedCtrl.LazyServiceProvider;
        lazyProvider.LazyGetRequiredService<IAttachmentUploadValidatorManager>().Returns(realValidator);
        await Assert.ThrowsAsync<BusinessException>(() =>
            blockedCtrl.SendUploadFileAsync(suId, CreateFormFile("payload.exe", "application/octet-stream", [0x4D, 0x5A]), 0, []));
    }
}
