using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.ScanCodes;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Json;
using Volo.Abp.Security.Claims;
using Xunit;

namespace IczpNet.Chat.QrCodes;

public class QrCode_Tests : ChatDomainTestBase
{
    private static readonly Guid DefaultUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

    private readonly IQrCodeManager _qrCodeManager;
    private readonly IScanCodeManager _scanCodeManager;
    private readonly IRepository<ChatObject, long> _chatObjectRepository;
    private readonly IRepository<ChatObjectType, string> _chatObjectTypeRepository;
    private readonly IRepository<QrCode, Guid> _qrCodeRepository;
    private readonly ISessionUnitRepository _sessionUnitRepository;
    private readonly IRepository<SessionRequest, Guid> _sessionRequestRepository;
    private readonly IRoomManager _roomManager;
    private readonly IDistributedCache<QrCodeCacheItem, QrCodeCacheKey> _qrCodeCache;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
    private readonly IJsonSerializer _jsonSerializer;

    protected override void AfterAddApplication(IServiceCollection services)
    {
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        multiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        services.AddSingleton(multiplexer);
        services.AddSingleton(Substitute.For<IBackgroundJobManager>());
    }

    public QrCode_Tests()
    {
        ServiceProvider.UseStaticObjectMapper();
        _qrCodeManager = GetRequiredService<IQrCodeManager>();
        _scanCodeManager = GetRequiredService<IScanCodeManager>();
        _chatObjectRepository = GetRequiredService<IRepository<ChatObject, long>>();
        _chatObjectTypeRepository = GetRequiredService<IRepository<ChatObjectType, string>>();
        _qrCodeRepository = GetRequiredService<IRepository<QrCode, Guid>>();
        _sessionUnitRepository = GetRequiredService<ISessionUnitRepository>();
        _sessionRequestRepository = GetRequiredService<IRepository<SessionRequest, Guid>>();
        _roomManager = GetRequiredService<IRoomManager>();
        _qrCodeCache = GetRequiredService<IDistributedCache<QrCodeCacheItem, QrCodeCacheKey>>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        _jsonSerializer = GetRequiredService<IJsonSerializer>();
    }

    private async Task<Dictionary<ChatObjectTypeEnums, ChatObjectType>> EnsureChatObjectTypesAsync()
    {
        var map = new Dictionary<ChatObjectTypeEnums, ChatObjectType>();
        foreach (var enumVal in new[]
        {
            ChatObjectTypeEnums.Personal,
            ChatObjectTypeEnums.Room,
            ChatObjectTypeEnums.ShopKeeper,
            ChatObjectTypeEnums.ShopWaiter,
            ChatObjectTypeEnums.Official
        })
        {
            var id = enumVal.ToString();
            var entity = await _chatObjectTypeRepository.FindAsync(id);
            if (entity == null)
            {
                entity = await _chatObjectTypeRepository.InsertAsync(new ChatObjectType(id)
                {
                    Name = id,
                    ObjectType = enumVal
                }, autoSave: true);
            }
            map[enumVal] = entity;
        }
        return map;
    }

    private async Task<ChatObject> CreateChatObjectAsync(
        Dictionary<ChatObjectTypeEnums, ChatObjectType> types,
        string name,
        ChatObjectTypeEnums type,
        Guid? appUserId,
        VerificationMethods verificationMethod = VerificationMethods.None)
    {
        var entity = new ChatObject(name, $"{type}_{Guid.NewGuid():N}"[..16], types[type], null);
        if (appUserId.HasValue)
        {
            entity.SetAppUserId(appUserId.Value);
        }
        entity.SetVerificationMethod(verificationMethod);
        return await _chatObjectRepository.InsertAsync(entity, autoSave: true);
    }

    [Fact]
    public async Task Generate_MultiChatObject_Permanent_Temporary_And_CustomDays_Should_Work()
    {
        long personalId = 0;
        long shopKeeperId = 0;
        long otherUserObjId = 0;

        await WithUnitOfWorkAsync(async () =>
        {
            var types = await EnsureChatObjectTypesAsync();
            personalId = (await CreateChatObjectAsync(types, "AlicePersonal", ChatObjectTypeEnums.Personal, DefaultUserId)).Id;
            shopKeeperId = (await CreateChatObjectAsync(types, "AliceShop", ChatObjectTypeEnums.ShopKeeper, DefaultUserId)).Id;
            otherUserObjId = (await CreateChatObjectAsync(types, "BobPersonal", ChatObjectTypeEnums.Personal, Guid.NewGuid())).Id;
        });

        // 1. 个人名片默认永久，重复获取应复用同一条记录
        var permanent1 = await _qrCodeManager.GenerateCardAsync(personalId);
        permanent1.Scene.ShouldBe(QrCodeScenes.ChatObjectCard);
        permanent1.OwnerId.ShouldBe(personalId);
        permanent1.OwnerObjectType.ShouldBe(ChatObjectTypeEnums.Personal);
        permanent1.IsPermanent.ShouldBeTrue();
        permanent1.ExpireDays.ShouldBeNull();
        permanent1.ExpirationTime.ShouldBeNull();
        permanent1.Content.ShouldBe($"gotoim://qr/{permanent1.Code}");

        var permanent2 = await _qrCodeManager.GenerateCardAsync(personalId);
        permanent2.Id.ShouldBe(permanent1.Id);
        permanent2.Code.ShouldBe(permanent1.Code);

        // 2. 7天临时名片
        var temp7 = await _qrCodeManager.GenerateCardAsync(personalId, isPermanent: false);
        temp7.Id.ShouldNotBe(permanent1.Id);
        temp7.IsPermanent.ShouldBeFalse();
        temp7.ExpireDays.ShouldBe(7);
        temp7.ExpirationTime.ShouldNotBeNull();

        // 3. 同用户第二个 ChatObjectId 自定义 30 天名片
        var custom30 = await _qrCodeManager.GenerateCardAsync(shopKeeperId, isPermanent: false, expireDays: 30);
        custom30.OwnerId.ShouldBe(shopKeeperId);
        custom30.OwnerObjectType.ShouldBe(ChatObjectTypeEnums.ShopKeeper);
        custom30.IsPermanent.ShouldBeFalse();
        custom30.ExpireDays.ShouldBe(30);
        custom30.ExpirationTime.ShouldNotBeNull();

        // 4. 非本人名下的 ChatObjectId 不允许生成名片
        await Should.ThrowAsync<AbpAuthorizationException>(async () =>
            await _qrCodeManager.GenerateCardAsync(otherUserObjId));
    }

    [Fact]
    public async Task Generate_GroupInvite_Should_Enforce_NonPermanent_And_MemberCheck()
    {
        long ownerId = 0;
        long memberId = 0;
        long outsiderId = 0;
        Guid outsiderUserId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var types = await EnsureChatObjectTypesAsync();
            ownerId = (await CreateChatObjectAsync(types, "RoomOwner", ChatObjectTypeEnums.Personal, DefaultUserId)).Id;
            memberId = (await CreateChatObjectAsync(types, "RoomMember", ChatObjectTypeEnums.Personal, Guid.NewGuid())).Id;
            outsiderId = (await CreateChatObjectAsync(types, "Outsider", ChatObjectTypeEnums.Personal, outsiderUserId)).Id;
        });

        var room = await _roomManager.CreateAsync("QrTestRoom", new List<long> { memberId }, ownerId);

        // 即使入参传 isPermanent: true，群聊二维码也必须强制为临时码（默认 7 天）
        var roomQr = await _qrCodeManager.GenerateCardAsync(room.Id, inviterId: ownerId, isPermanent: true);
        roomQr.Scene.ShouldBe(QrCodeScenes.GroupInvite);
        roomQr.OwnerId.ShouldBe(room.Id);
        roomQr.InviterId.ShouldBe(ownerId);
        roomQr.IsPermanent.ShouldBeFalse();
        roomQr.ExpireDays.ShouldBe(7);
        roomQr.ExpirationTime.ShouldNotBeNull();

        // 非群成员（切换当前用户为 outsider）不能为该群生成二维码
        var outsiderPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, outsiderUserId.ToString())
        }));
        using (_currentPrincipalAccessor.Change(outsiderPrincipal))
        {
            await Should.ThrowAsync<UserFriendlyException>(async () =>
                await _qrCodeManager.GenerateCardAsync(room.Id, inviterId: outsiderId));
        }
    }

    [Fact]
    public async Task Reset_And_Disable_Should_Invalidate_Previous_Code_And_Remove_Cache()
    {
        long ownerId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = await EnsureChatObjectTypesAsync();
            ownerId = (await CreateChatObjectAsync(types, "ResetOwner", ChatObjectTypeEnums.Personal, DefaultUserId)).Id;
        });

        var card1 = await _qrCodeManager.GenerateCardAsync(ownerId);

        // 触发一次解析以写入 Redis/DistributedCache
        var resolved1 = await _qrCodeManager.ResolveByCodeAsync(card1.Code);
        resolved1.IsValid.ShouldBeTrue();
        (await _qrCodeCache.GetAsync(new QrCodeCacheKey(card1.Code))).ShouldNotBeNull();

        // 重置名片
        var card2 = await _qrCodeManager.ResetCardAsync(ownerId);
        card2.Id.ShouldNotBe(card1.Id);
        card2.Code.ShouldNotBe(card1.Code);

        // 旧码缓存应被移除且解析标记为已停用
        (await _qrCodeCache.GetAsync(new QrCodeCacheKey(card1.Code))).ShouldBeNull();
        var resolvedOld = await _qrCodeManager.ResolveByCodeAsync(card1.Code);
        resolvedOld.IsValid.ShouldBeFalse();
        resolvedOld.IsDisabled.ShouldBeTrue();

        // 停用新码
        await _qrCodeManager.ResolveByCodeAsync(card2.Code);
        (await _qrCodeCache.GetAsync(new QrCodeCacheKey(card2.Code))).ShouldNotBeNull();

        await _qrCodeManager.DisableAsync(card2.Id);
        (await _qrCodeCache.GetAsync(new QrCodeCacheKey(card2.Code))).ShouldBeNull();
        var resolvedDisabled = await _qrCodeManager.ResolveByCodeAsync(card2.Code);
        resolvedDisabled.IsValid.ShouldBeFalse();
        resolvedDisabled.IsDisabled.ShouldBeTrue();
    }

    [Fact]
    public async Task ScanCodeManager_Should_Recognize_Template_Https_Expired_Legacy_And_ScanLogin()
    {
        long ownerId = 0;
        long viewerId = 0;
        Guid viewerUserId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var types = await EnsureChatObjectTypesAsync();
            ownerId = (await CreateChatObjectAsync(types, "ScanTarget", ChatObjectTypeEnums.Personal, DefaultUserId)).Id;
            viewerId = (await CreateChatObjectAsync(types, "ScanViewer", ChatObjectTypeEnums.Personal, viewerUserId)).Id;
        });

        var card = await _qrCodeManager.GenerateCardAsync(ownerId, isPermanent: false, expireDays: 7);

        var viewerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, viewerUserId.ToString())
        }));
        using (_currentPrincipalAccessor.Change(viewerPrincipal))
        {
            // 1. 默认模板 gotoim://qr/{code}
            var scan1 = await _scanCodeManager.ScanCodeAsync("QR_CODE", $"gotoim://qr/{card.Code}", viewerId);
            var qrHandler1 = scan1.ScanHandlerList.FirstOrDefault(x => x.HandlerFullName.Contains(nameof(QrCodeScanCodeHandler)));
            qrHandler1.ShouldNotBeNull();
            qrHandler1.Success.ShouldBeTrue();
            qrHandler1.Action.ShouldBe("chat_object_card");

            var info1 = _jsonSerializer.Deserialize<QrCodeScanDataInfo>(qrHandler1.Result);
            info1.IsValid.ShouldBeTrue();
            info1.OwnerId.ShouldBe(ownerId);
            info1.OwnerName.ShouldBe("ScanTarget");
            info1.ViewerChatObjectId.ShouldBe(viewerId);
            info1.IsSelf.ShouldBeFalse();

            // 2. HTTPS 短链识别
            var scanHttps = await _scanCodeManager.ScanCodeAsync("QR_CODE", $"https://chat.example.com/q/{card.Code}", viewerId);
            var qrHandlerHttps = scanHttps.ScanHandlerList.First(x => x.HandlerFullName.Contains(nameof(QrCodeScanCodeHandler)));
            qrHandlerHttps.Success.ShouldBeTrue();
            _jsonSerializer.Deserialize<QrCodeScanDataInfo>(qrHandlerHttps.Result).Code.ShouldBe(card.Code);

            // 3. 历史 ID 格式兼容识别 gotoim://user/?id={id}
            var legacyInfo = await _qrCodeManager.ResolveScanAsync($"gotoim://user/?id={ownerId}", viewerId);
            legacyInfo.ShouldNotBeNull();
            legacyInfo.IsValid.ShouldBeTrue();
            legacyInfo.OwnerId.ShouldBe(ownerId);
            legacyInfo.Action.ShouldBe("chat_object_card");

            // 4. 过期状态识别
            await WithUnitOfWorkAsync(async () =>
            {
                var entity = await _qrCodeRepository.GetAsync(card.Id);
                entity.SetValidity(false, 7, DateTime.UtcNow.AddMinutes(-10));
                await _qrCodeRepository.UpdateAsync(entity, autoSave: true);
            });
            await _qrCodeCache.RemoveAsync(new QrCodeCacheKey(card.Code));

            var expiredInfo = await _qrCodeManager.ResolveByCodeAsync(card.Code, viewerId);
            expiredInfo.IsValid.ShouldBeFalse();
            expiredInfo.IsExpired.ShouldBeTrue();

            // 5. 扫码登录处理器兼容
            var scanLogin = await _scanCodeManager.ScanCodeAsync("QR_CODE", "gotoim://scan-login?code=ticket-abc-123");
            var loginHandler = scanLogin.ScanHandlerList.FirstOrDefault(x => x.HandlerFullName.Contains(nameof(ScanLoginScanCodeHandler)));
            loginHandler.ShouldNotBeNull();
            loginHandler.Success.ShouldBeTrue();
            loginHandler.Action.ShouldBe("scan-login");
        }
    }

    [Fact]
    public async Task ApplyAsync_Room_And_Friend_Should_Be_Idempotent_And_Record_JoinWay()
    {
        long ownerId = 0;
        long joinerId = 0;
        long approvalFriendId = 0;
        Guid joinerUserId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var types = await EnsureChatObjectTypesAsync();
            ownerId = (await CreateChatObjectAsync(types, "GroupOwner", ChatObjectTypeEnums.Personal, DefaultUserId, VerificationMethods.None)).Id;
            joinerId = (await CreateChatObjectAsync(types, "Joiner", ChatObjectTypeEnums.Personal, joinerUserId, VerificationMethods.None)).Id;
            approvalFriendId = (await CreateChatObjectAsync(types, "StrictUser", ChatObjectTypeEnums.Personal, DefaultUserId, VerificationMethods.Required)).Id;
        });

        // 1. 无需验证的群聊扫码入群 + 幂等重复扫码
        var room = await _roomManager.CreateAsync("OpenRoom", new List<long>(), ownerId);
        await WithUnitOfWorkAsync(async () =>
        {
            var roomEntity = await _chatObjectRepository.GetAsync(room.Id);
            roomEntity.SetVerificationMethod(VerificationMethods.None);
            await _chatObjectRepository.UpdateAsync(roomEntity, autoSave: true);
        });

        var roomQr = await _qrCodeManager.GenerateCardAsync(room.Id, inviterId: ownerId);
        var strictCard = await _qrCodeManager.GenerateCardAsync(approvalFriendId);
        var openCard = await _qrCodeManager.GenerateCardAsync(ownerId);

        var joinerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, joinerUserId.ToString())
        }));
        using (_currentPrincipalAccessor.Change(joinerPrincipal))
        {
            var joinResult = await _qrCodeManager.ApplyAsync(roomQr.Code, joinerId);
            joinResult.Status.ShouldBe("joined");
            joinResult.SessionUnitId.ShouldNotBeNull();

            await WithUnitOfWorkAsync(async () =>
            {
                var ownerUnit = await _sessionUnitRepository.FindAsync(x => x.OwnerId == ownerId && x.DestinationId == room.Id);
                ownerUnit.ShouldNotBeNull();
                var joinedUnit = await _sessionUnitRepository.GetAsync(joinResult.SessionUnitId!.Value);
                joinedUnit.Setting.JoinWay.ShouldBe(JoinWays.Scan);
                joinedUnit.Setting.InviterId.ShouldBe(ownerUnit!.Id);
            });

            var joinAgain = await _qrCodeManager.ApplyAsync(roomQr.Code, joinerId);
            joinAgain.Status.ShouldBe("already_member");

            // 2. 需验证的个人名片扫码 -> 生成好友申请 SessionRequest
            var reqResult = await _qrCodeManager.ApplyAsync(strictCard.Code, joinerId, "我是扫码好友");
            reqResult.Status.ShouldBe("request_sent");
            reqResult.SessionRequestId.ShouldNotBeNull();
            await WithUnitOfWorkAsync(async () =>
            {
                (await _sessionRequestRepository.FindAsync(reqResult.SessionRequestId!.Value)).ShouldNotBeNull();
            });

            // 3. 无需验证的个人名片扫码 -> 直接加好友 + 幂等
            var addResult = await _qrCodeManager.ApplyAsync(openCard.Code, joinerId);
            addResult.Status.ShouldBe("added");
            addResult.SessionUnitId.ShouldNotBeNull();

            var addAgain = await _qrCodeManager.ApplyAsync(openCard.Code, joinerId);
            addAgain.Status.ShouldBe("already_friend");
        }
    }
}
