using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using IczpNet.Chat.ShortIds;
using IczpNet.Chat.UrlNormalizers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.QrCodes;

public class QrCodeManager(
    IRepository<QrCode, Guid> repository,
    IDistributedCache<QrCodeCacheItem, QrCodeCacheKey> qrCodeCache,
    IShortIdGenerator shortIdGenerator,
    IChatObjectManager chatObjectManager,
    IChatObjectRepository chatObjectRepository,
    ISessionUnitManager sessionUnitManager,
    ISessionUnitRepository sessionUnitRepository,
    IRepository<Session, Guid> sessionRepository,
    IRoomManager roomManager,
    ISessionRequestManager sessionRequestManager,
    IAbpDistributedLock distributedLock,
    IOptions<QrCodeOptions> options,
    ISettingProvider settingProvider,
    IConfiguration configuration,
    ICurrentUser currentUser,
    IObjectMapper<ChatDomainModule> objectMapper) : DomainService, IQrCodeManager
{
    protected IRepository<QrCode, Guid> Repository { get; } = repository;
    protected IDistributedCache<QrCodeCacheItem, QrCodeCacheKey> QrCodeCache { get; } = qrCodeCache;
    protected IShortIdGenerator ShortIdGenerator { get; } = shortIdGenerator;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IChatObjectRepository ChatObjectRepository { get; } = chatObjectRepository;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected IRepository<Session, Guid> SessionRepository { get; } = sessionRepository;
    protected IRoomManager RoomManager { get; } = roomManager;
    protected ISessionRequestManager SessionRequestManager { get; } = sessionRequestManager;
    protected IAbpDistributedLock DistributedLock { get; } = distributedLock;
    protected QrCodeOptions Config { get; } = options.Value;
    protected ISettingProvider SettingProvider { get; } = settingProvider;
    protected IUrlNormalizer UrlNormalizer => LazyServiceProvider.LazyGetService<IUrlNormalizer>();
    protected IHttpContextAccessor HttpContextAccessor => LazyServiceProvider.LazyGetService<IHttpContextAccessor>();
    protected IConfiguration Configuration { get; } = configuration;
    protected ICurrentUser CurrentUser { get; } = currentUser;
    protected IObjectMapper<ChatDomainModule> ObjectMapper { get; } = objectMapper;

    [UnitOfWork(isTransactional: true)]
    public virtual async Task<QrCode> GenerateCardAsync(
        long ownerId,
        long? inviterId = null,
        bool? isPermanent = null,
        int? expireDays = null,
        bool forceReset = false,
        string title = null,
        int? maxUseCount = null,
        string payloadJson = null)
    {
        var owner = await ChatObjectRepository.FindAsync(ownerId)
            ?? throw new UserFriendlyException("聊天对象不存在", "404");

        Assert.If(!owner.IsEnabled, "聊天对象已禁用，无法生成二维码");

        var scene = owner.ObjectType == ChatObjectTypeEnums.Room
            ? QrCodeScenes.GroupInvite
            : QrCodeScenes.ChatObjectCard;

        long? effectiveInviterId = inviterId;

        if (owner.ObjectType == ChatObjectTypeEnums.Room)
        {
            if (CurrentUser.Id.HasValue)
            {
                var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
                if (effectiveInviterId.HasValue)
                {
                    if (!ownedIds.Contains(effectiveInviterId.Value))
                    {
                        throw new AbpAuthorizationException("当前用户无权使用指定的群成员身份生成群二维码");
                    }
                }
                else
                {
                    effectiveInviterId = await FindActiveRoomMemberOwnerIdAsync(owner.Id, ownedIds);
                }
            }

            if (!effectiveInviterId.HasValue)
            {
                throw new UserFriendlyException("生成群二维码需要有效的群成员身份");
            }

            var inviterUnit = await FindActiveUnitAsync(effectiveInviterId.Value, owner.Id);
            if (inviterUnit == null)
            {
                throw new UserFriendlyException("您不是该群有效成员，无法生成群二维码");
            }
        }
        else
        {
            effectiveInviterId ??= owner.Id;

            if (CurrentUser.Id.HasValue)
            {
                var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
                var hasAccess = ownedIds.Contains(owner.Id)
                    || (owner.AppUserId.HasValue && owner.AppUserId.Value == CurrentUser.Id.Value)
                    || (owner.ParentId.HasValue && ownedIds.Contains(owner.ParentId.Value));

                if (!hasAccess)
                {
                    throw new AbpAuthorizationException("当前用户无权为该聊天对象生成二维码名片");
                }
            }
        }

        var effectiveTitle = string.IsNullOrWhiteSpace(title) ? owner.Name : title.Trim();

        return await GenerateInternalAsync(
            scene,
            owner.Id,
            owner.ObjectType,
            effectiveInviterId,
            isPermanent,
            expireDays,
            forceReset,
            effectiveTitle,
            maxUseCount,
            payloadJson);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task<QrCode> GenerateCustomAsync(
        QrCodeScenes scene,
        long? ownerId = null,
        long? inviterId = null,
        bool? isPermanent = null,
        int? expireDays = null,
        bool forceReset = false,
        string title = null,
        int? maxUseCount = null,
        string payloadJson = null)
    {
        if (scene is QrCodeScenes.ChatObjectCard or QrCodeScenes.GroupInvite)
        {
            Assert.If(!ownerId.HasValue, "生成名片或群二维码必须指定 OwnerId");
            return await GenerateCardAsync(
                ownerId!.Value,
                inviterId,
                isPermanent,
                expireDays,
                forceReset,
                title,
                maxUseCount,
                payloadJson);
        }

        ChatObjectTypeEnums? ownerObjectType = null;
        if (ownerId.HasValue)
        {
            var owner = await ChatObjectRepository.FindAsync(ownerId.Value)
                ?? throw new UserFriendlyException("聊天对象不存在", "404");
            ownerObjectType = owner.ObjectType;
        }

        return await GenerateInternalAsync(
            scene,
            ownerId,
            ownerObjectType,
            inviterId,
            isPermanent,
            expireDays,
            forceReset,
            title,
            maxUseCount,
            payloadJson);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual Task<QrCode> ResetCardAsync(
        long ownerId,
        long? inviterId = null,
        bool? isPermanent = null,
        int? expireDays = null,
        string title = null,
        int? maxUseCount = null,
        string payloadJson = null)
    {
        return GenerateCardAsync(
            ownerId,
            inviterId,
            isPermanent,
            expireDays,
            forceReset: true,
            title,
            maxUseCount,
            payloadJson);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task DisableAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        if (CurrentUser.Id.HasValue && entity.OwnerId.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            var isOwnerOrInviter = ownedIds.Contains(entity.OwnerId.Value)
                || (entity.InviterId.HasValue && ownedIds.Contains(entity.InviterId.Value))
                || entity.CreatorId == CurrentUser.Id.Value;

            if (!isOwnerOrInviter)
            {
                throw new AbpAuthorizationException("当前用户无权停用该二维码");
            }
        }

        entity.Disable();
        await Repository.UpdateAsync(entity, autoSave: true);
        await InvalidateCacheAsync(entity.Code);
    }

    [UnitOfWork]
    public virtual async Task<QrCodeCacheItem> GetCacheByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalizedCode = code.Trim();
        var cacheKey = new QrCodeCacheKey(normalizedCode);

        var cached = await QrCodeCache.GetAsync(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var entity = await Repository.FindAsync(x => x.Code == normalizedCode);
        if (entity == null)
        {
            return null;
        }

        var item = ObjectMapper.Map<QrCode, QrCodeCacheItem>(entity);
        await SetCacheAsync(item);
        return item;
    }

    [UnitOfWork]
    public virtual async Task<QrCodeScanDataInfo> ResolveScanAsync(string rawContent, long? viewerChatObjectId = null)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        var effectiveOptions = await GetEffectiveOptionsAsync();
        var parsed = QrCodeTemplateMatcher.ParseQrContent(rawContent, effectiveOptions);
        if (parsed == null)
        {
            return null;
        }

        var effectiveViewerId = await ResolveViewerChatObjectIdAsync(viewerChatObjectId);

        if (parsed.Kind == QrCodeParsedKind.LegacyChatObjectId && parsed.LegacyChatObjectId.HasValue)
        {
            return await BuildLegacyScanDataAsync(parsed.LegacyChatObjectId.Value, parsed.IsGroupHint, effectiveViewerId);
        }

        if (parsed.Kind == QrCodeParsedKind.CodeTicket && !string.IsNullOrWhiteSpace(parsed.Code))
        {
            return await ResolveByCodeCoreAsync(parsed.Code, effectiveViewerId);
        }

        return null;
    }

    [UnitOfWork]
    public virtual async Task<QrCodeScanDataInfo> ResolveByCodeAsync(string code, long? viewerChatObjectId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var effectiveViewerId = await ResolveViewerChatObjectIdAsync(viewerChatObjectId);
        return await ResolveByCodeCoreAsync(code.Trim(), effectiveViewerId);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task<QrCodeApplyResult> ApplyAsync(string code, long applicantChatObjectId, string requestMessage = null)
    {
        Assert.If(string.IsNullOrWhiteSpace(code), "二维码凭证不能为空");

        var applicant = await ChatObjectRepository.FindAsync(applicantChatObjectId)
            ?? throw new UserFriendlyException("当前聊天身份不存在", "404");

        Assert.If(!applicant.IsEnabled, "当前聊天身份已禁用");

        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            var hasAccess = ownedIds.Contains(applicant.Id)
                || (applicant.AppUserId.HasValue && applicant.AppUserId.Value == CurrentUser.Id.Value);
            if (!hasAccess)
            {
                throw new AbpAuthorizationException("当前用户无权使用该聊天身份操作");
            }
        }

        var normalizedCode = code.Trim();
        var qrCode = await Repository.FindAsync(x => x.Code == normalizedCode);
        if (qrCode == null)
        {
            throw new UserFriendlyException("二维码不存在或已失效");
        }

        if (!qrCode.IsEnabled)
        {
            throw new UserFriendlyException("该二维码已失效（已被重置或停用）");
        }

        if (qrCode.IsExpired(Clock.Now))
        {
            throw new UserFriendlyException(qrCode.Scene == QrCodeScenes.GroupInvite ? "该群二维码已过期" : "该名片二维码已过期");
        }

        if (qrCode.IsReachMaxUseCount())
        {
            throw new UserFriendlyException("该二维码已达到最大使用次数");
        }

        if (!qrCode.OwnerId.HasValue || qrCode.Scene is not (QrCodeScenes.ChatObjectCard or QrCodeScenes.GroupInvite))
        {
            throw new UserFriendlyException("该二维码不支持加好友或加入群聊");
        }

        var target = await ChatObjectRepository.FindAsync(qrCode.OwnerId.Value)
            ?? throw new UserFriendlyException("目标对象不存在或已注销");

        if (!target.IsEnabled)
        {
            throw new UserFriendlyException(target.ObjectType == ChatObjectTypeEnums.Room ? "该群聊已解散或停用" : "该账号已被禁用");
        }

        Assert.If(applicant.Id == target.Id, "不能添加自己为好友");

        var existingUnit = await FindActiveUnitAsync(applicant.Id, target.Id);
        if (existingUnit != null)
        {
            var isRoom = target.ObjectType == ChatObjectTypeEnums.Room;
            return new QrCodeApplyResult
            {
                Status = isRoom ? "already_member" : "already_friend",
                Message = isRoom ? "您已在群聊中" : "你们已经是好友了",
                SessionUnitId = existingUnit.Id,
                SessionId = existingUnit.SessionId,
                TargetChatObjectId = target.Id,
            };
        }

        if (existingUnit != null)
        {
            throw new UserFriendlyException("该会话关系已存在但处于不可用状态，请联系管理员或恢复会话");
        }

        QrCodeApplyResult result;

        if (target.ObjectType == ChatObjectTypeEnums.Room)
        {
            Assert.If(!await RoomManager.IsAllowJoinRoomAsync(applicant.ObjectType.GetValueOrDefault()),
                $"当前身份类型 [{applicant.ObjectType}] 不允许加入群聊");

            if (qrCode.InviterId.HasValue)
            {
                var inviterUnit = await SessionUnitManager.FindAsync(qrCode.InviterId.Value, target.Id);
                if (!IsActiveUnit(inviterUnit))
                {
                    throw new UserFriendlyException("邀请人已不在群聊中，该群二维码已失效");
                }
            }

            if (target.VerificationMethod == VerificationMethods.None)
            {
                var joinedUnits = await RoomManager.InviteAsync(new InviteInput
                {
                    RoomId = target.Id,
                    MemberIdList = [applicant.Id],
                    InviterId = qrCode.InviterId,
                    JoinWay = JoinWays.Scan,
                }, autoSendMessage: true);

                var joinedUnit = joinedUnits.FirstOrDefault();
                result = new QrCodeApplyResult
                {
                    Status = "joined",
                    Message = "已加入群聊",
                    SessionUnitId = joinedUnit?.Id,
                    SessionId = joinedUnit?.SessionId,
                    TargetChatObjectId = target.Id,
                };
            }
            else
            {
                var req = await SessionRequestManager.CreateRequestAsync(
                    applicant.Id,
                    target.Id,
                    string.IsNullOrWhiteSpace(requestMessage) ? "通过扫描群二维码申请加入" : requestMessage.Trim());

                result = new QrCodeApplyResult
                {
                    Status = "request_sent",
                    Message = "已提交入群申请，等待管理员审批",
                    SessionRequestId = req.Id,
                    TargetChatObjectId = target.Id,
                };
            }
        }
        else
        {
            var req = await SessionRequestManager.CreateRequestAsync(
                applicant.Id,
                target.Id,
                string.IsNullOrWhiteSpace(requestMessage) ? "通过扫描二维码名片添加" : requestMessage.Trim());

            if (req.IsHandled && req.IsAgreed == true)
            {
                var createdUnit = await SessionUnitManager.FindAsync(applicant.Id, target.Id);
                result = new QrCodeApplyResult
                {
                    Status = "added",
                    Message = "已添加为好友",
                    SessionUnitId = createdUnit?.Id,
                    SessionId = createdUnit?.SessionId,
                    SessionRequestId = req.Id,
                    TargetChatObjectId = target.Id,
                };
            }
            else
            {
                result = new QrCodeApplyResult
                {
                    Status = "request_sent",
                    Message = "已发送好友验证请求",
                    SessionRequestId = req.Id,
                    TargetChatObjectId = target.Id,
                };
            }
        }

        qrCode.IncrementUsedCount();
        await Repository.UpdateAsync(qrCode, autoSave: true);
        await SetCacheAsync(ObjectMapper.Map<QrCode, QrCodeCacheItem>(qrCode));

        return result;
    }

    public virtual async Task<string> FormatContentAsync(string code)
    {
        var template = await GetEffectiveContentTemplateAsync();
        return QrCodeTemplateMatcher.FormatContent(template, code);
    }

    [UnitOfWork(isTransactional: true)]
    protected virtual async Task<QrCode> GenerateInternalAsync(
        QrCodeScenes scene,
        long? ownerId,
        ChatObjectTypeEnums? ownerObjectType,
        long? inviterId,
        bool? isPermanent,
        int? expireDays,
        bool forceReset,
        string title,
        int? maxUseCount,
        string payloadJson)
    {
        var now = Clock.Now;
        var (effectiveIsPermanent, effectiveExpireDays, effectiveExpirationTime) =
            await ResolveValidityAsync(scene, ownerObjectType, isPermanent, expireDays, now);

        var lockKey = $"Chat:QrCode:Generate:{(int)scene}:{ownerId ?? 0}:{inviterId ?? 0}";
        await using var handle = await DistributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(5));
        if (handle == null)
        {
            throw new UserFriendlyException("生成二维码过于频繁，请稍后重试");
        }

        var baseQuery = (await Repository.GetQueryableAsync())
            .Where(x => x.Scene == scene
                     && x.OwnerId == ownerId
                     && x.InviterId == inviterId
                     && x.IsEnabled);

        if (!forceReset)
        {
            var minRemainingHours = effectiveIsPermanent
                ? 0d
                : Math.Min(
                    Math.Max(0, Config.ReuseMinRemainingHours),
                    Math.Max(1, effectiveExpireDays ?? 7) * 24.0 * 0.3);
            var minReusableExpiration = now.AddHours(minRemainingHours);

            var existingCandidates = await AsyncExecuter.ToListAsync(
                baseQuery.Where(x => x.IsPermanent == effectiveIsPermanent)
                         .Where(x => effectiveIsPermanent || (x.ExpirationTime != null && x.ExpirationTime > minReusableExpiration))
                         .OrderByDescending(x => x.CreationTime));

            var reusable = existingCandidates.FirstOrDefault(x =>
                x.ExpireDays == effectiveExpireDays
                && string.Equals(x.PayloadJson, payloadJson, StringComparison.Ordinal)
                && !x.IsReachMaxUseCount());

            if (reusable != null)
            {
                var latestContent = await FormatContentAsync(reusable.Code);
                if (!string.Equals(reusable.Content, latestContent, StringComparison.Ordinal))
                {
                    reusable.SetContent(latestContent);
                    await Repository.UpdateAsync(reusable, autoSave: true);
                }

                await SetCacheAsync(ObjectMapper.Map<QrCode, QrCodeCacheItem>(reusable));
                return reusable;
            }

            // 未命中可复用码（例如切换了临时天数 7天->30天，或原临时码临近过期）：
            // 自动停用同类型（临时/永久）的旧有效码并清理缓存，避免长期积累多张无效/冗余码
            var supersededCodes = await AsyncExecuter.ToListAsync(
                baseQuery.Where(x => x.IsPermanent == effectiveIsPermanent));
            if (supersededCodes.Count > 0)
            {
                foreach (var oldCode in supersededCodes)
                {
                    oldCode.Disable();
                    await InvalidateCacheAsync(oldCode.Code);
                }

                await Repository.UpdateManyAsync(supersededCodes, autoSave: true);
            }
        }
        else
        {
            var oldActiveCodes = await AsyncExecuter.ToListAsync(baseQuery);
            if (oldActiveCodes.Count > 0)
            {
                foreach (var oldCode in oldActiveCodes)
                {
                    oldCode.Disable();
                    await InvalidateCacheAsync(oldCode.Code);
                }

                await Repository.UpdateManyAsync(oldActiveCodes, autoSave: true);
            }
        }

        var code = await CreateUniqueCodeAsync();
        var content = await FormatContentAsync(code);

        var entity = new QrCode(GuidGenerator.Create(), code, scene);
        entity.SetContent(content);
        entity.SetOwner(ownerId, ownerObjectType);
        entity.SetInviterId(inviterId);
        entity.SetValidity(effectiveIsPermanent, effectiveExpireDays, effectiveExpirationTime);
        entity.SetTitle(title);
        entity.SetMaxUseCount(maxUseCount);
        entity.SetPayloadJson(payloadJson);

        entity = await Repository.InsertAsync(entity, autoSave: true);
        await SetCacheAsync(ObjectMapper.Map<QrCode, QrCodeCacheItem>(entity));

        return entity;
    }

    protected virtual async Task<(bool IsPermanent, int? ExpireDays, DateTime? ExpirationTime)> ResolveValidityAsync(
        QrCodeScenes scene,
        ChatObjectTypeEnums? ownerObjectType,
        bool? isPermanent,
        int? expireDays,
        DateTime now)
    {
        var maxExpireDays = await SettingProvider.GetAsync(ChatSettings.QrCodeMaxExpireDays, Config.MaxExpireDays);
        if (maxExpireDays <= 0)
        {
            maxExpireDays = Config.MaxExpireDays > 0 ? Config.MaxExpireDays : 365;
        }

        if (expireDays.HasValue && expireDays.Value > maxExpireDays)
        {
            throw new UserFriendlyException($"二维码有效天数不能超过 {maxExpireDays} 天");
        }

        if (scene == QrCodeScenes.GroupInvite || ownerObjectType == ChatObjectTypeEnums.Room)
        {
            var defaultGroupDays = await SettingProvider.GetAsync(ChatSettings.QrCodeGroupExpireDays, Config.DefaultGroupExpireDays);
            if (defaultGroupDays <= 0)
            {
                defaultGroupDays = Config.DefaultGroupExpireDays > 0 ? Config.DefaultGroupExpireDays : 7;
            }

            var days = expireDays.HasValue && expireDays.Value > 0 ? expireDays.Value : defaultGroupDays;
            if (days > maxExpireDays)
            {
                throw new UserFriendlyException($"群二维码有效天数不能超过 {maxExpireDays} 天");
            }

            return (false, days, now.AddDays(days));
        }

        if (expireDays.HasValue && expireDays.Value > 0)
        {
            return (false, expireDays.Value, now.AddDays(expireDays.Value));
        }

        if (isPermanent == false)
        {
            var defaultTempDays = await SettingProvider.GetAsync(ChatSettings.QrCodeDefaultExpireDays, Config.DefaultTemporaryExpireDays);
            if (defaultTempDays <= 0)
            {
                defaultTempDays = Config.DefaultTemporaryExpireDays > 0 ? Config.DefaultTemporaryExpireDays : 7;
            }

            return (false, defaultTempDays, now.AddDays(defaultTempDays));
        }

        return (true, null, null);
    }

    protected virtual async Task<string> CreateUniqueCodeAsync()
    {
        for (var i = 0; i < 5; i++)
        {
            var candidate = await ShortIdGenerator.CreateAsync();
            if (!await Repository.AnyAsync(x => x.Code == candidate))
            {
                return candidate;
            }
        }

        return Guid.NewGuid().ToString("N")[..16];
    }

    protected virtual async Task<QrCodeScanDataInfo> ResolveByCodeCoreAsync(string code, long? effectiveViewerId)
    {
        var cacheItem = await GetCacheByCodeAsync(code);
        if (cacheItem == null)
        {
            return new QrCodeScanDataInfo
            {
                Code = code,
                Scene = QrCodeScenes.ChatObjectCard,
                Action = "chat_object_card",
                IsValid = false,
                IsDisabled = true,
                InvalidReason = "二维码不存在或已失效",
                ViewerChatObjectId = effectiveViewerId,
            };
        }

        var now = Clock.Now;
        var isExpired = cacheItem.IsExpired(now);
        var isReachMax = cacheItem.IsReachMaxUseCount();

        if (cacheItem.Scene is QrCodeScenes.WorkbenchApp or QrCodeScenes.CustomPayload)
        {
            var actionName = cacheItem.Scene == QrCodeScenes.WorkbenchApp ? "workbench_app" : "custom_payload";
            var customValid = cacheItem.IsEnabled && !isExpired && !isReachMax;
            string customReason = null;
            if (!cacheItem.IsEnabled) customReason = "该二维码已停用";
            else if (isExpired) customReason = "该二维码已过期";
            else if (isReachMax) customReason = "该二维码已达到使用次数上限";

            return new QrCodeScanDataInfo
            {
                QrCodeId = cacheItem.Id,
                Code = cacheItem.Code,
                Scene = cacheItem.Scene,
                Action = actionName,
                IsValid = customValid,
                IsExpired = isExpired,
                IsDisabled = !cacheItem.IsEnabled || isReachMax,
                InvalidReason = customReason,
                IsPermanent = cacheItem.IsPermanent,
                ExpireDays = cacheItem.ExpireDays,
                ExpirationTime = cacheItem.ExpirationTime,
                OwnerId = cacheItem.OwnerId,
                OwnerName = cacheItem.Title,
                OwnerObjectType = cacheItem.OwnerObjectType,
                InviterId = cacheItem.InviterId,
                ViewerChatObjectId = effectiveViewerId,
                PayloadJson = cacheItem.PayloadJson,
            };
        }

        ChatObject owner = null;
        if (cacheItem.OwnerId.HasValue)
        {
            owner = await ChatObjectRepository.FindAsync(cacheItem.OwnerId.Value);
        }

        var isGroup = cacheItem.Scene == QrCodeScenes.GroupInvite || owner?.ObjectType == ChatObjectTypeEnums.Room;
        var action = isGroup ? "group_invite" : "chat_object_card";

        var result = new QrCodeScanDataInfo
        {
            QrCodeId = cacheItem.Id,
            Code = cacheItem.Code,
            Scene = cacheItem.Scene,
            Action = action,
            IsPermanent = cacheItem.IsPermanent,
            ExpireDays = cacheItem.ExpireDays,
            ExpirationTime = cacheItem.ExpirationTime,
            OwnerId = cacheItem.OwnerId,
            InviterId = cacheItem.InviterId,
            ViewerChatObjectId = effectiveViewerId,
            PayloadJson = cacheItem.PayloadJson,
        };

        if (owner != null)
        {
            await PopulateOwnerInfoAsync(result, owner, effectiveViewerId);
        }

        if (cacheItem.InviterId.HasValue)
        {
            var inviter = await ChatObjectRepository.FindAsync(cacheItem.InviterId.Value);
            result.InviterName = inviter?.DisplayName ?? inviter?.Name;
        }

        if (owner == null || !owner.IsEnabled)
        {
            result.IsValid = false;
            result.IsDisabled = true;
            result.InvalidReason = isGroup ? "该群聊不存在或已解散" : "该账号不存在或已禁用";
        }
        else if (!cacheItem.IsEnabled)
        {
            result.IsValid = false;
            result.IsDisabled = true;
            result.InvalidReason = "该二维码已失效（已被重置或停用）";
        }
        else if (isExpired)
        {
            result.IsValid = false;
            result.IsExpired = true;
            result.InvalidReason = isGroup ? "该群二维码已过期" : "该名片二维码已过期";
        }
        else if (isReachMax)
        {
            result.IsValid = false;
            result.IsDisabled = true;
            result.InvalidReason = "该二维码已达到使用次数上限";
        }
        else
        {
            result.IsValid = true;
        }

        return result;
    }

    protected virtual async Task<QrCodeScanDataInfo> BuildLegacyScanDataAsync(
        long targetChatObjectId,
        bool isGroupHint,
        long? effectiveViewerId)
    {
        var owner = await ChatObjectRepository.FindAsync(targetChatObjectId);
        var isGroup = owner?.ObjectType == ChatObjectTypeEnums.Room || (owner == null && isGroupHint);

        var result = new QrCodeScanDataInfo
        {
            Scene = isGroup ? QrCodeScenes.GroupInvite : QrCodeScenes.ChatObjectCard,
            Action = isGroup ? "group_invite" : "chat_object_card",
            IsPermanent = true,
            OwnerId = targetChatObjectId,
            ViewerChatObjectId = effectiveViewerId,
        };

        if (owner == null || !owner.IsEnabled)
        {
            result.IsValid = false;
            result.IsDisabled = true;
            result.InvalidReason = isGroup ? "该群聊不存在或已解散" : "该账号不存在或已禁用";
            return result;
        }

        await PopulateOwnerInfoAsync(result, owner, effectiveViewerId);
        result.IsValid = true;
        return result;
    }

    protected virtual async Task PopulateOwnerInfoAsync(
        QrCodeScanDataInfo result,
        ChatObject owner,
        long? effectiveViewerId)
    {
        result.OwnerId = owner.Id;
        result.OwnerName = owner.DisplayName ?? owner.Name;
        result.OwnerCode = owner.Code;
        result.OwnerPortrait = SafeNormalizeUrl(owner.Portrait);
        result.OwnerThumbnail = SafeNormalizeUrl(owner.Thumbnail);
        result.OwnerObjectType = owner.ObjectType;
        result.OwnerObjectTypeDescription = owner.ObjectTypeDescription;
        result.Gender = owner.Gender;
        result.Description = owner.Description;
        result.VerificationMethod = owner.VerificationMethod;
        result.NeedsApproval = owner.VerificationMethod != VerificationMethods.None;

        var isRoom = owner.ObjectType == ChatObjectTypeEnums.Room;
        if (isRoom)
        {
            var roomSession = await SessionRepository.FindAsync(x => x.OwnerId == owner.Id);
            if (roomSession != null)
            {
                result.SessionId = roomSession.Id;
            }

            var memberQuery = (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.DestinationId == owner.Id && x.OwnerId != owner.Id)
                .Where(SessionUnit.GetActivePredicate(Clock.Now));
            result.MemberCount = await AsyncExecuter.CountAsync(memberQuery);
        }

        if (effectiveViewerId.HasValue && effectiveViewerId.Value > 0)
        {
            result.IsSelf = owner.Id == effectiveViewerId.Value;

            var viewerUnit = await FindActiveUnitAsync(effectiveViewerId.Value, owner.Id);
            if (viewerUnit != null)
            {
                result.SessionUnitId = viewerUnit.Id;
                result.SessionId ??= viewerUnit.SessionId;
                if (isRoom)
                {
                    result.IsAlreadyMember = true;
                }
                else
                {
                    result.IsFriend = true;
                }
            }
        }
    }

    protected virtual async Task<long?> ResolveViewerChatObjectIdAsync(long? requestedViewerId)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return requestedViewerId;
        }

        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (ownedIds == null || ownedIds.Count == 0)
        {
            return null;
        }

        if (requestedViewerId.HasValue && ownedIds.Contains(requestedViewerId.Value))
        {
            return requestedViewerId.Value;
        }

        return ownedIds[0];
    }

    protected virtual async Task<long?> FindActiveRoomMemberOwnerIdAsync(long roomId, List<long> candidateOwnerIds)
    {
        if (candidateOwnerIds == null || candidateOwnerIds.Count == 0)
        {
            return null;
        }

        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.DestinationId == roomId && candidateOwnerIds.Contains(x.OwnerId))
            .Where(SessionUnit.GetActivePredicate(Clock.Now))
            .Select(x => (long?)x.OwnerId);

        return await AsyncExecuter.FirstOrDefaultAsync(query);
    }

    protected virtual async Task<string> GetEffectiveContentTemplateAsync()
    {
        var settingTemplate = await SettingProvider.GetOrNullAsync(ChatSettings.QrCodeContentTemplate);
        if (!string.IsNullOrWhiteSpace(settingTemplate) &&
            settingTemplate.Contains(QrCodeTemplateMatcher.CodePlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            return settingTemplate.Trim();
        }

        return string.IsNullOrWhiteSpace(Config.ContentTemplate)
            ? "gotoim://qr/{code}"
            : Config.ContentTemplate.Trim();
    }

    protected virtual async Task<QrCodeOptions> GetEffectiveOptionsAsync()
    {
        var settingTemplate = await SettingProvider.GetOrNullAsync(ChatSettings.QrCodeContentTemplate);
        var templates = new List<string>();
        if (!string.IsNullOrWhiteSpace(settingTemplate))
        {
            templates.Add(settingTemplate.Trim());
        }
        if (!string.IsNullOrWhiteSpace(Config.ContentTemplate))
        {
            templates.Add(Config.ContentTemplate.Trim());
        }
        if (Config.CompatibleTemplates != null)
        {
            templates.AddRange(Config.CompatibleTemplates);
        }

        return new QrCodeOptions
        {
            ContentTemplate = await GetEffectiveContentTemplateAsync(),
            CompatibleTemplates = templates.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ScanLoginTemplate = Config.ScanLoginTemplate,
            ScanLoginCompatibleTemplates = Config.ScanLoginCompatibleTemplates,
            DefaultTemporaryExpireDays = Config.DefaultTemporaryExpireDays,
            DefaultGroupExpireDays = Config.DefaultGroupExpireDays,
            MaxExpireDays = Config.MaxExpireDays,
            CacheExpirationHours = Config.CacheExpirationHours,
        };
    }

    protected virtual async Task SetCacheAsync(QrCodeCacheItem item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Code))
        {
            return;
        }

        var hours = Config.CacheExpirationHours > 0 ? Config.CacheExpirationHours : 24;
        var entryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(hours)
        };

        if (!item.IsPermanent && item.ExpirationTime.HasValue)
        {
            var remaining = item.ExpirationTime.Value - Clock.Now;
            if (remaining > TimeSpan.Zero && remaining < entryOptions.AbsoluteExpirationRelativeToNow.Value)
            {
                entryOptions.AbsoluteExpirationRelativeToNow = remaining;
            }
        }

        await QrCodeCache.SetAsync(new QrCodeCacheKey(item.Code), item, entryOptions);
    }

    protected virtual Task InvalidateCacheAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Task.CompletedTask;
        }

        return QrCodeCache.RemoveAsync(new QrCodeCacheKey(code.Trim()));
    }

    protected virtual string SafeNormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var hasSelfUrl = !string.IsNullOrWhiteSpace(Configuration["App:SelfUrl"]);
        if (!hasSelfUrl && HttpContextAccessor?.HttpContext == null)
        {
            return url;
        }

        return UrlNormalizer != null ? UrlNormalizer.Normalize(url) : url;
    }

    protected virtual async Task<SessionUnit> FindActiveUnitAsync(long ownerId, long destinationId)
    {
        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == ownerId && x.DestinationId == destinationId)
            .Where(SessionUnit.GetActivePredicate(Clock.Now));

        return await AsyncExecuter.FirstOrDefaultAsync(query);
    }

    protected static bool IsActiveUnit(SessionUnit unit)
        => unit != null && !unit.IsDeleted && unit.Setting != null && unit.Setting.IsEnabled && !unit.Setting.IsKilled;
}
