using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Controllers;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class FileControllerAuthorization_Tests
{
    private static (FileController Controller, IBlobAccessAuthorizationService AuthService, BlobSignatureService SignatureService) CreateController(
        Guid? userId,
        long[] ownedChatObjects,
        byte[] fileBytes = null)
    {
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        var sigOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions
        {
            SigningKey = "Http.Test.Signing.Key.2026",
            DefaultExpireSeconds = 300
        });
        var sigService = new BlobSignatureService(sigOptions, clock);
        var authService = Substitute.For<IBlobAccessAuthorizationService>();
        var directUrl = Substitute.For<IBlobDirectUrlProvider>();
        directUrl.TryGetPresignedUrlAsync(Arg.Any<Blob>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((string)null);

        var chatObjectManager = Substitute.For<IChatObjectManager>();
        if (userId.HasValue)
        {
            chatObjectManager.GetIdListByUserIdAsync(userId.Value).Returns(ownedChatObjects.ToList());
        }

        var sessionUnitManager = Substitute.For<ISessionUnitManager>();

        var controller = new FileController(
            authService,
            sigService,
            directUrl,
            chatObjectManager,
            sessionUnitManager,
            sigOptions);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        var blobManager = Substitute.For<IBlobManager>();
        blobManager.GetBytesAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(fileBytes ?? [1, 2, 3]);
        var blobResolver = Substitute.For<IBlobResolver>();
        blobResolver.GetFileUrlAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(call => $"/api/chat/file/{call.Arg<Guid>()}");

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
        lazy.LazyGetRequiredService<IBlobManager>().Returns(blobManager);
        lazy.LazyGetRequiredService<IBlobResolver>().Returns(blobResolver);
        controller.LazyServiceProvider = lazy;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = userId.HasValue
                    ? new ClaimsPrincipal(new ClaimsIdentity([new Claim(AbpClaimTypes.UserId, userId.Value.ToString())], "Test"))
                    : new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        return (controller, authService, sigService);
    }

    [Fact]
    public async Task GetAsync_supports_legacy_and_signed_requests_and_blocks_tampered_or_denied()
    {
        var userId = Guid.NewGuid();
        var blobId = Guid.NewGuid();
        var suId = Guid.NewGuid();
        var msgId = 9001L;
        var blob = new Blob(blobId, "chat-files") { Name = "2026/a.png", FileName = "a.png", MimeType = "image/png" };

        var (authCtrl, authSvc, sigSvc) = CreateController(userId, [10L]);
        authSvc.CheckAccessAsync(blobId, null, null, null, Arg.Any<IReadOnlyCollection<long>>(), false, Arg.Any<CancellationToken>())
            .Returns(blob);

        var legacyResult = await authCtrl.GetAsync(blobId);
        var fileResult = Assert.IsType<FileContentResult>(legacyResult);
        Assert.Equal("image/png", fileResult.ContentType);

        // 匿名签名请求验证
        var (anonCtrl, anonAuthSvc, _) = CreateController(null, []);
        var signedUrl = sigSvc.AppendSignature($"/api/chat/file/{blobId}", blobId, suId, msgId);
        var query = System.Web.HttpUtility.ParseQueryString(new Uri("https://example.com" + signedUrl).Query);
        var exp = long.Parse(query["exp"]!);
        var sig = query["sig"]!;

        anonAuthSvc.CheckAccessAsync(blobId, suId, msgId, null, Arg.Any<IReadOnlyCollection<long>>(), true, Arg.Any<CancellationToken>())
            .Returns(blob);
        var signedResult = await anonCtrl.GetAsync(blobId, suId, msgId, null, exp, sig);
        Assert.IsType<FileContentResult>(signedResult);

        // 篡改 msgId 签名返回 403
        var tamperedResult = await anonCtrl.GetAsync(blobId, suId, msgId + 1, null, exp, sig);
        var statusTampered = Assert.IsType<ObjectResult>(tamperedResult);
        Assert.Equal(StatusCodes.Status403Forbidden, statusTampered.StatusCode);

        // 无消息权限拒绝返回 403
        authSvc.CheckAccessAsync(blobId, suId, msgId, null, Arg.Any<IReadOnlyCollection<long>>(), false, Arg.Any<CancellationToken>())
            .Returns<Blob>(_ => throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied));
        var deniedResult = await authCtrl.GetAsync(blobId, suId, msgId);
        var statusDenied = Assert.IsType<ObjectResult>(deniedResult);
        Assert.Equal(StatusCodes.Status403Forbidden, statusDenied.StatusCode);
    }

    [Fact]
    public async Task SignAsync_reuses_BlobAccessAuthorizationService_and_blocks_unauthorized_message()
    {
        var userId = Guid.NewGuid();
        var blobId = Guid.NewGuid();
        var suId = Guid.NewGuid();
        var rootMsgId = 9999L;

        var (ctrl, authSvc, _) = CreateController(userId, [10L]);
        var suMgr = Substitute.For<ISessionUnitManager>();
        suMgr.GetCacheAsync(suId).Returns(new SessionUnitCacheItem { Id = suId, OwnerId = 10L });

        var chatObjMgr = Substitute.For<IChatObjectManager>();
        chatObjMgr.GetIdListByUserIdAsync(userId).Returns(new List<long> { 10L });

        var sigOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions { SigningKey = "Http.Test.Signing.Key.2026", DefaultExpireSeconds = 300 });
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        var sigSvc = new BlobSignatureService(sigOptions, clock);
        var directUrl = Substitute.For<IBlobDirectUrlProvider>();

        var realCtrl = new FileController(authSvc, sigSvc, directUrl, chatObjMgr, suMgr, sigOptions)
        {
            LazyServiceProvider = ctrl.LazyServiceProvider,
            ControllerContext = ctrl.ControllerContext
        };

        authSvc.CheckAccessAsync(blobId, suId, null, rootMsgId, Arg.Any<IReadOnlyCollection<long>>(), false, Arg.Any<CancellationToken>())
            .Returns<Blob>(_ => throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied));

        var forbidden = await realCtrl.SignAsync(blobId, suId, null, rootMsgId);
        var status = Assert.IsType<StatusCodeResult>(forbidden);
        Assert.Equal(StatusCodes.Status403Forbidden, status.StatusCode);

        authSvc.CheckAccessAsync(blobId, suId, null, rootMsgId, Arg.Any<IReadOnlyCollection<long>>(), false, Arg.Any<CancellationToken>())
            .Returns(new Blob(blobId, "chat-files"));

        var ok = await realCtrl.SignAsync(blobId, suId, null, rootMsgId);
        Assert.IsType<OkObjectResult>(ok);
    }

    [Fact]
    public async Task GetAsync_blocks_presigned_direct_redirect_for_private_blobs_unless_enabled()
    {
        var userId = Guid.NewGuid();
        var privateBlobId = Guid.NewGuid();
        var publicBlobId = Guid.NewGuid();
        var privateBlob = new Blob(privateBlobId, "chat-files") { Name = "2026/priv.png", FileName = "priv.png", MimeType = "image/png", IsPublic = false };
        var publicBlob = new Blob(publicBlobId, "chat-object-portraits") { Name = "2026/pub.png", FileName = "pub.png", MimeType = "image/png", IsPublic = true };

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        var sigOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions
        {
            SigningKey = "Http.Test.Signing.Key.2026",
            DefaultExpireSeconds = 900,
            EnableDirectUrlForPrivateBlobs = false,
            PrivateDirectUrlExpireSeconds = 45
        });
        var sigSvc = new BlobSignatureService(sigOptions, clock);
        var authSvc = Substitute.For<IBlobAccessAuthorizationService>();
        authSvc.CheckAccessAsync(privateBlobId, null, null, null, Arg.Any<IReadOnlyCollection<long>>(), false, Arg.Any<CancellationToken>())
            .Returns(privateBlob);
        authSvc.CheckAccessAsync(publicBlobId, null, null, null, Arg.Any<IReadOnlyCollection<long>>(), false, Arg.Any<CancellationToken>())
            .Returns(publicBlob);

        var directUrl = Substitute.For<IBlobDirectUrlProvider>();
        directUrl.TryGetPresignedUrlAsync(Arg.Any<Blob>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns("https://minio.local/presigned-url");

        var (baseCtrl, _, _) = CreateController(userId, [10L]);
        var controller = new FileController(
            authSvc,
            sigSvc,
            directUrl,
            Substitute.For<IChatObjectManager>(),
            Substitute.For<ISessionUnitManager>(),
            sigOptions)
        {
            LazyServiceProvider = baseCtrl.LazyServiceProvider,
            ControllerContext = baseCtrl.ControllerContext
        };

        // 私有附件默认不走预签名重定向，每次请求经过应用字节流
        var privateResult = await controller.GetAsync(privateBlobId);
        Assert.IsType<FileContentResult>(privateResult);
        await directUrl.DidNotReceive().TryGetPresignedUrlAsync(privateBlob, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());

        // 公开资源允许重定向
        var publicResult = await controller.GetAsync(publicBlobId);
        var redirect = Assert.IsType<RedirectResult>(publicResult);
        Assert.Equal("https://minio.local/presigned-url", redirect.Url);
    }
}
