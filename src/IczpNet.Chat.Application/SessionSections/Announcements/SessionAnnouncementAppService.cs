using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionSections.Announcements.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.TextTemplates;
using IczpNet.Chat.Ulids;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementAppService(
    ISessionAnnouncementRepository announcementRepository,
    ISessionAnnouncementReceiptRepository receiptRepository,
    ISessionUnitRepository sessionUnitRepository,
    ISessionAnnouncementManager announcementManager,
    ISessionPermissionChecker sessionPermissionChecker,
    IMessageManager messageManager,
    IDistributedCache<string> distributedCache = null,
    IRepository<SessionAnnouncementRevision, Guid> revisionRepository = null) : ChatAppService, ISessionAnnouncementAppService
{
    protected ISessionAnnouncementRepository AnnouncementRepository { get; } = announcementRepository;
    protected ISessionAnnouncementReceiptRepository ReceiptRepository { get; } = receiptRepository;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected ISessionAnnouncementManager AnnouncementManager { get; } = announcementManager;
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IMessageManager MessageManager { get; } = messageManager;
    protected IDistributedCache<string> DistributedCache { get; } = distributedCache;
    protected IRepository<SessionAnnouncementRevision, Guid> RevisionRepository { get; } = revisionRepository;

    private readonly record struct UnitPermSnapshot(
        bool IsCreator,
        bool Create,
        bool Publish,
        bool Approve,
        bool Update,
        bool UpdateOwn,
        bool UpdateAny,
        bool Delete,
        bool DeleteOwn,
        bool DeleteAny,
        bool Pin,
        bool Remind,
        bool ViewStat);

    private async Task<UnitPermSnapshot> GetUnitPermSnapshotAsync(SessionUnit unit)
    {
        var isCreator = unit.Setting != null && (unit.Setting.IsCreator || unit.Setting.IsStatic);
        if (isCreator)
        {
            return new UnitPermSnapshot(true, true, true, true, true, true, true, true, true, true, true, true, true);
        }

        var create = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Create, unit);
        var publish = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Publish, unit);
        var approve = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Approve, unit);
        var update = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, unit);
        var updateOwn = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn, unit);
        var updateAny = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny, unit);
        var delete = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Delete, unit);
        var deleteOwn = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteOwn, unit);
        var deleteAny = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny, unit);
        var pin = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, unit);
        var remind = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Remind, unit);
        var viewStat = await SessionPermissionChecker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat, unit);

        return new UnitPermSnapshot(false, create, publish || update, approve, update, updateOwn, updateAny, delete, deleteOwn, deleteAny, pin, remind, viewStat);
    }

    private async Task CheckAnyOrFallbackAsync(SessionUnit unit, params string[] permissions)
    {
        foreach (var perm in permissions)
        {
            if (await SessionPermissionChecker.IsGrantedAsync(perm, unit))
            {
                return;
            }
        }
        await SessionPermissionChecker.CheckAsync(permissions[0], unit);
    }

    private async Task CheckEditPermissionAsync(SessionAnnouncement entity, SessionUnit unit)
    {
        var isAuthor = entity.PublisherSessionUnitId == unit.Id || entity.PublisherChatObjectId == unit.OwnerId;
        var isCreator = unit.Setting != null && (unit.Setting.IsCreator || unit.Setting.IsStatic);
        if (isCreator)
        {
            return;
        }

        if (entity.Status == AnnouncementStatus.Draft)
        {
            if (!isAuthor)
            {
                throw new AbpAuthorizationException("草稿仅发布者本人可操作。");
            }
            await CheckAnyOrFallbackAsync(unit,
                SessionPermissionDefinitionConsts.AnnouncementPermission.Create,
                SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn,
                SessionPermissionDefinitionConsts.AnnouncementPermission.Update);
            return;
        }

        if (isAuthor)
        {
            if (entity.Status is AnnouncementStatus.PendingReview or AnnouncementStatus.Rejected)
            {
                await CheckAnyOrFallbackAsync(unit,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.Create,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.Update,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny);
                return;
            }

            await CheckAnyOrFallbackAsync(unit,
                SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn,
                SessionPermissionDefinitionConsts.AnnouncementPermission.Update,
                SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny);
            return;
        }

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny, unit);
    }

    private async Task CheckDeletePermissionAsync(SessionAnnouncement entity, SessionUnit unit)
    {
        var isAuthor = entity.PublisherSessionUnitId == unit.Id || entity.PublisherChatObjectId == unit.OwnerId;
        var isCreator = unit.Setting != null && (unit.Setting.IsCreator || unit.Setting.IsStatic);
        if (isCreator)
        {
            return;
        }

        if (entity.Status == AnnouncementStatus.Draft)
        {
            if (!isAuthor)
            {
                throw new AbpAuthorizationException("草稿仅发布者本人可删除。");
            }
            await CheckAnyOrFallbackAsync(unit,
                SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteOwn,
                SessionPermissionDefinitionConsts.AnnouncementPermission.Create,
                SessionPermissionDefinitionConsts.AnnouncementPermission.Delete,
                SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny);
            return;
        }

        if (isAuthor)
        {
            await CheckAnyOrFallbackAsync(unit,
                SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteOwn,
                SessionPermissionDefinitionConsts.AnnouncementPermission.Delete,
                SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny);
            return;
        }

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny, unit);
    }

    private static void ApplyCapabilities(SessionAnnouncementDto dto, SessionAnnouncement entity, SessionUnit unit, UnitPermSnapshot perms, bool isExpired)
    {
        var isCreator = perms.IsCreator;
        var isAuthor = entity.PublisherSessionUnitId == unit.Id || entity.PublisherChatObjectId == unit.OwnerId;

        dto.CanEdit = entity.Status == AnnouncementStatus.Draft
            ? (isCreator || isAuthor)
            : (isCreator || perms.UpdateAny || (isAuthor && (perms.UpdateOwn || perms.Update || (entity.Status is AnnouncementStatus.PendingReview or AnnouncementStatus.Rejected && perms.Create))));
        dto.CanDelete = entity.Status == AnnouncementStatus.Draft
            ? (isCreator || isAuthor)
            : (isCreator || perms.DeleteAny || (isAuthor && (perms.DeleteOwn || perms.Delete)));
        dto.CanPublish = (entity.Status is AnnouncementStatus.Draft or AnnouncementStatus.Rejected or AnnouncementStatus.Revoked)
            && (isCreator || perms.Publish || perms.Update);
        dto.CanSubmitReview = (entity.Status is AnnouncementStatus.Draft or AnnouncementStatus.Rejected)
            && (isAuthor || isCreator)
            && (perms.Create || perms.Publish || perms.UpdateOwn || perms.Update);
        dto.CanApprove = entity.Status == AnnouncementStatus.PendingReview && (isCreator || perms.Approve);
        dto.CanRevoke = entity.Status == AnnouncementStatus.Published
            && (isCreator || perms.UpdateAny || (isAuthor && (perms.UpdateOwn || perms.Update)));
        dto.CanPin = isCreator || perms.Pin;
        dto.CanRemind = entity.Status == AnnouncementStatus.Published && !isExpired && (isCreator || perms.Remind || perms.Update);
        dto.CanViewStat = isCreator || perms.ViewStat || perms.Update;
    }

    protected virtual async Task<SessionUnit> GetSessionUnitOrDefaultAsync(Guid sessionUnitId, Guid? sessionId = null)
    {
        if (!CurrentUser.Id.HasValue)
            throw new AbpAuthorizationException("Authentication required.");

        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Setting)
            .Include(x => x.Owner)
            .Include(x => x.GrantList)
            .Include(x => x.SessionUnitRoleList).ThenInclude(x => x.SessionRole).ThenInclude(x => x.GrantList)
            .AsSplitQuery();
        SessionUnit unit = null;
        if (sessionUnitId != Guid.Empty)
        {
            unit = await query.FirstOrDefaultAsync(x => x.Id == sessionUnitId);
        }
        if (unit == null && sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            unit = await query.Where(x => x.SessionId == sessionId.Value && ownedIds.Contains(x.OwnerId)
                    && x.Setting.IsEnabled && !x.Setting.IsKilled)
                .OrderByDescending(x => x.Setting.IsCreator).ThenByDescending(x => x.SessionUnitRoleList.Any()).ThenBy(x => x.Id)
                .FirstOrDefaultAsync();
            if (unit == null)
            {
                unit = await query.Where(x => x.Id == sessionId.Value && ownedIds.Contains(x.OwnerId)
                        && x.Setting.IsEnabled && !x.Setting.IsKilled)
                    .FirstOrDefaultAsync();
            }
        }
        if (unit == null && sessionUnitId != Guid.Empty)
        {
            // 兼容客户端把 SessionId 误传入 sessionUnitId 字段的场景
            unit = await query.Where(x => x.SessionId == sessionUnitId && ownedIds.Contains(x.OwnerId)
                    && x.Setting.IsEnabled && !x.Setting.IsKilled)
                .OrderByDescending(x => x.Setting.IsCreator).ThenByDescending(x => x.SessionUnitRoleList.Any()).ThenBy(x => x.Id)
                .FirstOrDefaultAsync();
        }
        if (unit == null)
        {
            throw new AbpAuthorizationException("A valid session membership is required.");
        }

        if (!ownedIds.Contains(unit.OwnerId) || unit.IsDeleted || unit.Setting == null
            || !unit.Setting.IsEnabled || unit.Setting.IsKilled || !unit.SessionId.HasValue
            || (sessionId.HasValue && sessionId.Value != Guid.Empty && unit.SessionId != sessionId && unit.Id != sessionId)
            || (unit.DestinationObjectType != ChatObjectTypeEnums.Room && unit.DestinationObjectType != ChatObjectTypeEnums.Square))
            throw new AbpAuthorizationException("Invalid announcement membership.");

        return unit;
    }

    protected virtual async Task<IAbpDistributedLockHandle> LockSessionAsync(Guid sessionId)
    {
        var distributedLock = LazyServiceProvider.LazyGetRequiredService<IAbpDistributedLock>();
        var handle = await distributedLock.TryAcquireAsync($"Chat:Announcement:Session:{sessionId:N}", TimeSpan.FromSeconds(10));
        return handle ?? throw new UserFriendlyException("公告正在更新，请重试。");
    }

    public virtual async Task<SessionAnnouncementDetailDto> GetPinnedAsync(Guid sessionId)
    {
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, sessionId);
        var targetSessionId = unit.SessionId!.Value;
        var entity = await AnnouncementRepository.GetPinnedAsync(targetSessionId);
        if (entity == null)
        {
            return null;
        }

        return await MapToDetailDtoAsync(entity, unit);
    }

    public virtual async Task<List<SessionAnnouncementDetailDto>> GetPinnedListAsync(Guid sessionId)
    {
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, sessionId);
        var targetSessionId = unit.SessionId!.Value;
        var entities = await AnnouncementRepository.GetPinnedListAsync(targetSessionId);
        var result = new List<SessionAnnouncementDetailDto>(entities.Count);
        foreach (var entity in entities)
        {
            result.Add(await MapToDetailDtoAsync(entity, unit));
        }
        return result;
    }

    public virtual async Task<SessionAnnouncementDetailDto> GetAsync(Guid id)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, entity.SessionId);
        if (entity.Status != AnnouncementStatus.Published)
        {
            var isAuthor = entity.PublisherSessionUnitId == unit.Id || entity.PublisherChatObjectId == unit.OwnerId;
            if (!isAuthor)
            {
                if (entity.Status == AnnouncementStatus.PendingReview || entity.Status == AnnouncementStatus.Rejected)
                {
                    await CheckAnyOrFallbackAsync(unit,
                        SessionPermissionDefinitionConsts.AnnouncementPermission.Approve,
                        SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny,
                        SessionPermissionDefinitionConsts.AnnouncementPermission.Update);
                }
                else
                {
                    await CheckAnyOrFallbackAsync(unit,
                        SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny,
                        SessionPermissionDefinitionConsts.AnnouncementPermission.Update);
                }
            }
        }
        return await MapToDetailDtoAsync(entity, unit);
    }

    public virtual async Task<SessionAnnouncementPermissionsDto> GetPermissionsAsync(Guid sessionId)
    {
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, sessionId);
        var targetSessionId = unit.SessionId!.Value;
        var perms = await GetUnitPermSnapshotAsync(unit);

        var queryable = await AnnouncementRepository.GetQueryableAsync();
        var pendingCount = 0;
        if (perms.IsCreator || perms.Approve)
        {
            pendingCount = await queryable.CountAsync(x => x.SessionId == targetSessionId && x.Status == AnnouncementStatus.PendingReview);
        }
        else if (perms.Create)
        {
            pendingCount = await queryable.CountAsync(x => x.SessionId == targetSessionId
                && x.Status == AnnouncementStatus.PendingReview
                && (x.PublisherSessionUnitId == unit.Id || x.PublisherChatObjectId == unit.OwnerId));
        }

        var draftCount = await queryable.CountAsync(x => x.SessionId == targetSessionId
            && x.Status == AnnouncementStatus.Draft
            && (perms.IsCreator || x.PublisherSessionUnitId == unit.Id || x.PublisherChatObjectId == unit.OwnerId));

        return new SessionAnnouncementPermissionsDto
        {
            SessionId = targetSessionId,
            SessionUnitId = unit.Id,
            IsCreator = perms.IsCreator,
            CanCreate = perms.IsCreator || perms.Create || perms.Publish || perms.Update,
            CanPublish = perms.IsCreator || perms.Publish || perms.Update,
            CanApprove = perms.IsCreator || perms.Approve,
            CanUpdateAny = perms.IsCreator || perms.UpdateAny,
            CanDeleteAny = perms.IsCreator || perms.DeleteAny,
            CanPin = perms.IsCreator || perms.Pin,
            CanRemind = perms.IsCreator || perms.Remind || perms.Update,
            CanViewStat = perms.IsCreator || perms.ViewStat || perms.Update,
            PendingReviewCount = pendingCount,
            DraftCount = draftCount
        };
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input)
    {
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, input.SessionId);
        var targetSessionId = unit.SessionId!.Value;
        var queryable = await AnnouncementRepository.GetQueryableAsync();
        var perms = await GetUnitPermSnapshotAsync(unit);

        if (!input.IncludeDrafts)
        {
            queryable = queryable.Where(x => x.SessionId == targetSessionId && x.Status == AnnouncementStatus.Published);
        }
        else
        {
            var canApproveAll = perms.IsCreator || perms.Approve;
            var canManageAll = perms.IsCreator || perms.UpdateAny;
            queryable = queryable.Where(x => x.SessionId == targetSessionId && (
                x.Status == AnnouncementStatus.Published
                || (x.Status == AnnouncementStatus.Draft && (perms.IsCreator || x.PublisherSessionUnitId == unit.Id || x.PublisherChatObjectId == unit.OwnerId))
                || (x.Status == AnnouncementStatus.PendingReview && (canApproveAll || canManageAll || x.PublisherSessionUnitId == unit.Id || x.PublisherChatObjectId == unit.OwnerId))
                || (x.Status == AnnouncementStatus.Rejected && (canApproveAll || canManageAll || x.PublisherSessionUnitId == unit.Id || x.PublisherChatObjectId == unit.OwnerId))
                || (x.Status == AnnouncementStatus.Revoked && (canManageAll || x.PublisherSessionUnitId == unit.Id || x.PublisherChatObjectId == unit.OwnerId))
            ));
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.IsPinned.HasValue)
        {
            queryable = queryable.Where(x => x.IsPinned == input.IsPinned.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x => x.Title.Contains(keyword) || x.Content.Contains(keyword));
        }

        var totalCount = await queryable.CountAsync();

        var entities = await queryable
            .OrderByDescending(x => x.Sorting)
            .ThenByDescending(x => x.PinnedTime ?? x.CreationTime)
            .ThenByDescending(x => x.Id)
            .PageBy(input)
            .ToListAsync();

        var dtoList = new List<SessionAnnouncementDto>();
        var relatedUnitIds = entities.Select(x => x.PublisherSessionUnitId)
            .Concat(entities.Where(x => x.ReviewerSessionUnitId.HasValue).Select(x => x.ReviewerSessionUnitId!.Value))
            .Distinct()
            .ToList();
        var unitDict = await (await SessionUnitRepository.GetQueryableAsync()).Where(x => relatedUnitIds.Contains(x.Id))
            .Include(x => x.Owner).Include(x => x.Setting).ToDictionaryAsync(x => x.Id);
        var ids = entities.Select(x => x.Id).ToList();
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id!.Value);
        var confirmedReceipts = await (await ReceiptRepository.GetQueryableAsync())
            .Where(x => ids.Contains(x.AnnouncementId) && ownedIds.Contains(x.ChatObjectId) && x.IsConfirmed)
            .GroupBy(x => x.AnnouncementId)
            .Select(g => new { AnnouncementId = g.Key, LatestConfirmedTime = g.Max(x => x.ConfirmedTime), MaxConfirmedVersion = g.Max(x => x.ConfirmedVersion) })
            .ToDictionaryAsync(x => x.AnnouncementId, x => x);

        var unitJoinTime = unit.Setting?.JoinTime ?? unit.CreationTime;
        var now = Clock.Now;
        foreach (var entity in entities)
        {
            unitDict.TryGetValue(entity.PublisherSessionUnitId, out var publisher);
            SessionUnit reviewer = null;
            if (entity.ReviewerSessionUnitId.HasValue)
            {
                unitDict.TryGetValue(entity.ReviewerSessionUnitId.Value, out reviewer);
            }
            var dto = MapToDto(entity, publisher, reviewer, unitJoinTime, now);
            ApplyCapabilities(dto, entity, unit, perms, dto.IsExpired);
            if (confirmedReceipts.TryGetValue(entity.Id, out var receiptInfo))
            {
                dto.ConfirmedVersion = receiptInfo.MaxConfirmedVersion;
                dto.IsConfirmedByCurrentUser = receiptInfo.MaxConfirmedVersion >= entity.Version
                    && (!entity.ApplyToNewMembers || !receiptInfo.LatestConfirmedTime.HasValue || receiptInfo.LatestConfirmedTime.Value >= unitJoinTime);
            }
            dtoList.Add(dto);
        }

        return new PagedResultDto<SessionAnnouncementDto>(totalCount, dtoList);
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementDto>> SearchAsync(SearchSessionAnnouncementInput input)
    {
        if (!CurrentUser.Id.HasValue || string.IsNullOrWhiteSpace(input.Keyword))
        {
            return new PagedResultDto<SessionAnnouncementDto>(0, []);
        }

        var keyword = input.Keyword.Trim();
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (ownedIds == null || ownedIds.Count == 0)
        {
            return new PagedResultDto<SessionAnnouncementDto>(0, []);
        }

        var activeUnitsQuery = (await SessionUnitRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => ownedIds.Contains(x.OwnerId)
                && x.SessionId != null
                && x.Setting.IsEnabled
                && !x.Setting.IsKilled);

        if (input.SessionId.HasValue)
        {
            activeUnitsQuery = activeUnitsQuery.Where(x => x.SessionId == input.SessionId.Value);
        }

        var unitInfos = await activeUnitsQuery
            .Select(x => new
            {
                SessionId = x.SessionId.Value,
                SessionName = x.Destination != null ? x.Destination.Name : null,
                JoinTime = x.Setting.JoinTime ?? x.CreationTime
            })
            .ToListAsync();

        var sessionMap = unitInfos
            .GroupBy(x => x.SessionId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(u => u.JoinTime).First());

        var sessionIds = sessionMap.Keys.ToList();
        if (sessionIds.Count == 0)
        {
            return new PagedResultDto<SessionAnnouncementDto>(0, []);
        }

        var queryable = (await AnnouncementRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.SessionId.HasValue
                && sessionIds.Contains(x.SessionId.Value)
                && x.Status == AnnouncementStatus.Published
                && (x.Title.Contains(keyword) || x.Content.Contains(keyword)));

        var totalCount = await queryable.CountAsync();
        var entities = await queryable
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.Id)
            .PageBy(input)
            .ToListAsync();

        var publisherIds = entities.Select(x => x.PublisherSessionUnitId).Distinct().ToList();
        var publishers = await (await SessionUnitRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => publisherIds.Contains(x.Id))
            .Include(x => x.Owner)
            .Include(x => x.Setting)
            .ToDictionaryAsync(x => x.Id);

        var ids = entities.Select(x => x.Id).ToList();
        var confirmedIds = await (await ReceiptRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => ids.Contains(x.AnnouncementId) && ownedIds.Contains(x.ChatObjectId) && x.IsConfirmed)
            .Select(x => x.AnnouncementId)
            .Distinct()
            .ToListAsync();

        var now = Clock.Now;
        var dtoList = new List<SessionAnnouncementDto>(entities.Count);
        foreach (var entity in entities)
        {
            publishers.TryGetValue(entity.PublisherSessionUnitId, out var publisher);
            sessionMap.TryGetValue(entity.SessionId!.Value, out var unitInfo);
            var dto = MapToDto(entity, publisher, null, unitInfo?.JoinTime, now);
            dto.SessionName = unitInfo?.SessionName;
            dto.IsConfirmedByCurrentUser = confirmedIds.Contains(entity.Id);
            dtoList.Add(dto);
        }

        return new PagedResultDto<SessionAnnouncementDto>(totalCount, dtoList);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> CreateAsync(CreateSessionAnnouncementInput input)
    {
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, input.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Create,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Publish,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update);
        if (input.IsPinned || (input.Sorting.HasValue && input.Sorting.Value > 0))
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        var perms = await GetUnitPermSnapshotAsync(sessionUnit);
        var hasPublishPerm = perms.IsCreator || perms.Publish || perms.Update;
        var sessionId = sessionUnit.SessionId!.Value;
        var chatObjectId = sessionUnit.DestinationId ?? sessionUnit.OwnerId;

        await using var handle = await LockSessionAsync(sessionId);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        var isScheduledFuture = input.PublishTime.HasValue && input.PublishTime.Value > Clock.Now;
        AnnouncementStatus targetStatus;
        bool isApproved;
        if (input.SaveAsDraft)
        {
            targetStatus = AnnouncementStatus.Draft;
            isApproved = hasPublishPerm;
        }
        else if (input.SubmitForReview || !hasPublishPerm)
        {
            // 无直接发布权时强制进入待审核队列
            targetStatus = AnnouncementStatus.PendingReview;
            isApproved = false;
        }
        else if (isScheduledFuture)
        {
            targetStatus = AnnouncementStatus.Draft;
            isApproved = true;
        }
        else
        {
            targetStatus = AnnouncementStatus.Published;
            isApproved = true;
        }

        var entity = await AnnouncementManager.CreateAsync(
            sessionId: sessionId,
            chatObjectId: chatObjectId,
            publisherSessionUnitId: sessionUnit.Id,
            publisherChatObjectId: sessionUnit.OwnerId,
            title: input.Title,
            content: input.Content,
            editorType: input.EditorType,
            isPinned: input.IsPinned,
            isPopup: input.IsPopup,
            requireConfirm: input.RequireConfirm,
            sorting: input.Sorting,
            expireTime: input.ExpireTime,
            applyToNewMembers: input.ApplyToNewMembers,
            status: targetStatus,
            publishTime: input.PublishTime,
            isApproved: isApproved);

        if (input.SendMessageNotification && targetStatus == AnnouncementStatus.Published)
        {
            await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: false);
        }

        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity, sessionUnit);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> UpdateAsync(Guid id, UpdateSessionAnnouncementInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await CheckEditPermissionAsync(entity, sessionUnit);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        if (input.ExpectedVersion.HasValue && entity.Version != input.ExpectedVersion.Value)
        {
            throw new UserFriendlyException($"公告已被其他人修改（当前版本 v{entity.Version}），请刷新后重试。");
        }
        if (input.IsPinned != entity.IsPinned || (input.Sorting.HasValue && input.Sorting.Value != entity.Sorting))
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        var perms = await GetUnitPermSnapshotAsync(sessionUnit);
        var hasPublishPerm = perms.IsCreator || perms.Publish || perms.Update;
        var isScheduledFuture = input.PublishTime.HasValue && input.PublishTime.Value > Clock.Now;

        AnnouncementStatus nextStatus;
        if (input.SaveAsDraft)
        {
            nextStatus = AnnouncementStatus.Draft;
            entity.SetApproved(hasPublishPerm);
        }
        else if (input.SubmitForReview || (!hasPublishPerm && entity.Status != AnnouncementStatus.Published))
        {
            nextStatus = AnnouncementStatus.PendingReview;
            entity.SetApproved(false);
        }
        else if (isScheduledFuture)
        {
            nextStatus = AnnouncementStatus.Draft;
            entity.SetApproved(true);
        }
        else
        {
            if (!hasPublishPerm && entity.Status != AnnouncementStatus.Published)
            {
                nextStatus = AnnouncementStatus.PendingReview;
                entity.SetApproved(false);
            }
            else
            {
                nextStatus = AnnouncementStatus.Published;
                entity.SetApproved(true);
            }
        }

        entity = await AnnouncementManager.UpdateAsync(
            announcement: entity,
            title: input.Title,
            content: input.Content,
            editorType: input.EditorType,
            isPinned: input.IsPinned,
            isPopup: input.IsPopup,
            requireConfirm: input.RequireConfirm,
            sorting: input.Sorting,
            expireTime: input.ExpireTime,
            applyToNewMembers: input.ApplyToNewMembers,
            status: nextStatus,
            publishTime: input.PublishTime,
            resetReceipts: input.ResetReceipts,
            bumpVersion: input.BumpVersion,
            changeSummary: input.ChangeSummary,
            editorSessionUnitId: sessionUnit.Id,
            editorChatObjectId: sessionUnit.OwnerId);

        if (input.SendMessageNotification && entity.Status == AnnouncementStatus.Published)
        {
            await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: true);
        }

        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity, sessionUnit);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> SubmitReviewAsync(Guid id, SubmitAnnouncementReviewInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await CheckEditPermissionAsync(entity, sessionUnit);
        if (entity.Status != AnnouncementStatus.Draft && entity.Status != AnnouncementStatus.Rejected)
        {
            throw new UserFriendlyException("仅草稿或已驳回状态的公告可提交审核。");
        }

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.SubmitForReviewAsync(entity);
        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity, sessionUnit);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> ReviewAsync(Guid id, ReviewSessionAnnouncementInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        // 只要拥有审核管理权限，允许审核自己提审的公告
        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Approve,
            SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);

        await AnnouncementManager.ReviewAsync(
            announcement: entity,
            approved: input.Approved,
            reviewerSessionUnitId: sessionUnit.Id,
            reviewerChatObjectId: sessionUnit.OwnerId,
            remark: input.Remark);

        if (input.Approved && input.SendMessageNotification && entity.Status == AnnouncementStatus.Published)
        {
            await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: entity.Version > 1);
        }

        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity, sessionUnit);
    }

    public virtual async Task<List<SessionAnnouncementRevisionDto>> GetRevisionsAsync(Guid id)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, entity.SessionId);
        if (entity.Status != AnnouncementStatus.Published)
        {
            var isAuthor = entity.PublisherSessionUnitId == unit.Id || entity.PublisherChatObjectId == unit.OwnerId;
            if (!isAuthor)
            {
                await CheckAnyOrFallbackAsync(unit,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.Approve,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny,
                    SessionPermissionDefinitionConsts.AnnouncementPermission.Update);
            }
        }

        if (RevisionRepository == null)
        {
            return [];
        }

        var revisions = await (await RevisionRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.AnnouncementId == id)
            .OrderByDescending(x => x.Version)
            .ToListAsync();

        var editorUnitIds = revisions.Select(x => x.EditorSessionUnitId).Distinct().ToList();
        var editorUnits = await (await SessionUnitRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => editorUnitIds.Contains(x.Id))
            .Include(x => x.Owner)
            .Include(x => x.Setting)
            .ToDictionaryAsync(x => x.Id);

        return revisions.Select(r =>
        {
            editorUnits.TryGetValue(r.EditorSessionUnitId, out var eu);
            return new SessionAnnouncementRevisionDto
            {
                Id = r.Id,
                AnnouncementId = r.AnnouncementId,
                Version = r.Version,
                Title = r.Title,
                Content = r.Content,
                EditorType = r.EditorType,
                EditorSessionUnitId = r.EditorSessionUnitId,
                EditorChatObjectId = r.EditorChatObjectId,
                EditorName = eu?.DisplayName ?? eu?.Owner?.Name,
                EditorPortrait = eu?.Owner?.Portrait,
                ChangeSummary = r.ChangeSummary,
                CreationTime = r.CreationTime
            };
        }).ToList();
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> PublishAsync(Guid id, Guid sessionUnitId)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Publish,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Approve,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.PublishAsync(entity);
        await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: false);
        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity, sessionUnit);
    }

    [HttpPost]
    public virtual async Task RevokeAsync(Guid id, Guid sessionUnitId)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await CheckEditPermissionAsync(entity, sessionUnit);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.RevokeAsync(entity);
        await uow.CompleteAsync();
    }

    [HttpPost]
    public virtual async Task<int> RemindUnconfirmedAsync(Guid id, RemindAnnouncementInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        Check.NotNull(entity.SessionId, nameof(entity.SessionId));

        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));
        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Remind,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update);

        if (entity.Status != AnnouncementStatus.Published)
        {
            throw new UserFriendlyException("仅已发布公告支持发送催办提醒");
        }
        if (entity.ExpireTime.HasValue && entity.ExpireTime.Value <= Clock.Now)
        {
            throw new UserFriendlyException("该公告已过期，无法发送催办提醒");
        }

        var cooldownKey = $"Chat_Announcement_RemindCooldown:{id}";
        var lastRemind = DistributedCache != null ? await DistributedCache.GetAsync(cooldownKey) : null;
        if (!string.IsNullOrEmpty(lastRemind))
        {
            throw new UserFriendlyException("催办过于频繁，请 60 分钟后再试");
        }

        var members = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled && !x.Setting.IsKilled);
        if (!entity.ApplyToNewMembers)
        {
            members = members.Where(x => (x.Setting.JoinTime ?? x.Setting.CreationTime) <= entity.CreationTime);
        }
        var confirmedUnitIds = (await ReceiptRepository.GetQueryableAsync())
            .Where(x => x.AnnouncementId == id && x.IsConfirmed && x.ConfirmedVersion >= entity.Version)
            .Select(x => x.SessionUnitId);
        var unconfirmedCount = await members.CountAsync(x => !confirmedUnitIds.Contains(x.Id));

        if (unconfirmedCount <= 0)
        {
            return 0;
        }

        await SendAnnouncementRemindMessageAsync(sessionUnit, entity, unconfirmedCount);
        if (DistributedCache != null)
        {
            await DistributedCache.SetAsync(
                cooldownKey,
                Clock.Now.ToString("O"),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });
        }

        return unconfirmedCount;
    }

    [HttpPost]
    public virtual async Task SetPinnedAsync(Guid id, SetPinnedInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.SetPinnedAsync(entity, input.IsPinned, input.Sorting);
        await uow.CompleteAsync();
    }

    [HttpPost]
    public virtual async Task DeleteAsync(Guid id, Guid sessionUnitId)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await CheckDeletePermissionAsync(entity, sessionUnit);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.DeleteAsync(entity);
        await uow.CompleteAsync();
    }

    [HttpPost]
    public virtual async Task ConfirmReceiptAsync(Guid id, ConfirmSessionAnnouncementInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await AnnouncementManager.ConfirmReceiptAsync(
            announcementId: id,
            sessionUnitId: sessionUnit.Id,
            chatObjectId: sessionUnit.OwnerId,
            deviceId: input.DeviceId);
    }

    [HttpPost]
    public virtual async Task RecordViewAsync(Guid id, RecordAnnouncementViewInput input)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await AnnouncementManager.RecordViewAsync(
            announcementId: id,
            sessionUnitId: sessionUnit.Id,
            chatObjectId: sessionUnit.OwnerId,
            deviceId: input.DeviceId);
    }

    public virtual async Task<SessionAnnouncementStatDto> GetStatAsync(Guid id, Guid sessionUnitId)
    {
        var announcement = await AnnouncementRepository.GetAsync(id);
        Check.NotNull(announcement.SessionId, nameof(announcement.SessionId));

        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, announcement.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));
        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update);

        var members = (await SessionUnitRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.SessionId == announcement.SessionId && x.Setting.IsEnabled && !x.Setting.IsKilled);
        if (!announcement.ApplyToNewMembers)
        {
            members = members.Where(x => (x.Setting.JoinTime ?? x.Setting.CreationTime) <= announcement.CreationTime);
        }

        var countCacheKey = $"Chat_Announcement_MemberCount:{announcement.SessionId}:{announcement.ApplyToNewMembers}:{announcement.CreationTime.Ticks}";
        var cachedCountStr = DistributedCache != null ? await DistributedCache.GetAsync(countCacheKey) : null;
        int totalMembers;
        if (int.TryParse(cachedCountStr, out var parsedCount))
        {
            totalMembers = parsedCount;
        }
        else
        {
            totalMembers = await members.CountAsync();
            if (DistributedCache != null)
            {
                await DistributedCache.SetAsync(
                    countCacheKey,
                    totalMembers.ToString(),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
            }
        }

        var receiptStats = await (await ReceiptRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.AnnouncementId == id && members.Any(u => u.Id == x.SessionUnitId))
            .GroupBy(x => 1)
            .Select(g => new
            {
                ReadCount = g.Count(),
                ConfirmedCount = g.Count(x => x.IsConfirmed && x.ConfirmedVersion >= announcement.Version)
            })
            .FirstOrDefaultAsync();

        var readCount = receiptStats?.ReadCount ?? 0;
        var confirmedCount = receiptStats?.ConfirmedCount ?? 0;

        return new SessionAnnouncementStatDto
        {
            AnnouncementId = id,
            TotalMembersCount = totalMembers,
            ReadCount = readCount,
            UnreadCount = Math.Max(0, totalMembers - readCount),
            ConfirmedCount = confirmedCount,
            UnconfirmedCount = Math.Max(0, totalMembers - confirmedCount)
        };
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementMemberDto>> GetMembersAsync(Guid id, GetAnnouncementMemberListInput input)
    {
        var announcement = await AnnouncementRepository.GetAsync(id);
        Check.NotNull(announcement.SessionId, nameof(announcement.SessionId));

        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, announcement.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));
        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update);

        var receiptsQuery = (await ReceiptRepository.GetQueryableAsync()).Where(x => x.AnnouncementId == id);
        var unitQuery = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == announcement.SessionId && x.Setting.IsEnabled && !x.Setting.IsKilled);
        if (!announcement.ApplyToNewMembers)
        {
            unitQuery = unitQuery.Where(x => (x.Setting.JoinTime ?? x.Setting.CreationTime) <= announcement.CreationTime);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            unitQuery = unitQuery.Where(x => (x.Setting.MemberName != null && x.Setting.MemberName.Contains(keyword)) ||
                                             (x.Owner != null && x.Owner.Name.Contains(keyword)));
        }

        switch (input.FilterType)
        {
            case AnnouncementMemberFilterType.Read:
                var readUnitIds = receiptsQuery.Select(x => x.SessionUnitId);
                unitQuery = unitQuery.Where(x => readUnitIds.Contains(x.Id));
                break;
            case AnnouncementMemberFilterType.Unread:
                var recordedUnitIds = receiptsQuery.Select(x => x.SessionUnitId);
                unitQuery = unitQuery.Where(x => !recordedUnitIds.Contains(x.Id));
                break;
            case AnnouncementMemberFilterType.Confirmed:
                var confirmedUnitIds = receiptsQuery.Where(x => x.IsConfirmed && x.ConfirmedVersion >= announcement.Version).Select(x => x.SessionUnitId);
                unitQuery = unitQuery.Where(x => confirmedUnitIds.Contains(x.Id));
                break;
            case AnnouncementMemberFilterType.Unconfirmed:
                var alreadyConfirmedUnitIds = receiptsQuery.Where(x => x.IsConfirmed && x.ConfirmedVersion >= announcement.Version).Select(x => x.SessionUnitId);
                unitQuery = unitQuery.Where(x => !alreadyConfirmedUnitIds.Contains(x.Id));
                break;
        }

        var totalCount = await unitQuery.CountAsync();

        if (input.CursorJoinTime.HasValue && input.CursorSessionUnitId.HasValue)
        {
            var cursorTime = input.CursorJoinTime.Value;
            var cursorId = input.CursorSessionUnitId.Value;
            unitQuery = unitQuery.Where(x =>
                (x.Setting.JoinTime ?? x.Setting.CreationTime) < cursorTime ||
                ((x.Setting.JoinTime ?? x.Setting.CreationTime) == cursorTime && x.Id.CompareTo(cursorId) < 0));
        }

        var orderedQuery = unitQuery
            .AsNoTracking()
            .OrderByDescending(x => x.Setting.JoinTime ?? x.Setting.CreationTime)
            .ThenByDescending(x => x.Id);

        var pagedUnits = (input.CursorJoinTime.HasValue && input.CursorSessionUnitId.HasValue)
            ? await orderedQuery.Take(input.MaxResultCount).Include(x => x.Owner).Include(x => x.Setting).ToListAsync()
            : await orderedQuery.PageBy(input).Include(x => x.Owner).Include(x => x.Setting).ToListAsync();

        var unitIds = pagedUnits.Select(x => x.Id).ToList();
        var receipts = await receiptsQuery.Where(x => unitIds.Contains(x.SessionUnitId)).ToListAsync();
        var receiptDict = receipts.ToDictionary(x => x.SessionUnitId, x => x);

        var list = pagedUnits.Select(u =>
        {
            receiptDict.TryGetValue(u.Id, out var r);
            return new SessionAnnouncementMemberDto
            {
                SessionUnitId = u.Id,
                ChatObjectId = u.OwnerId,
                MemberName = u.DisplayName ?? u.Owner?.Name,
                MemberPortrait = u.Owner?.Portrait,
                IsRead = r != null,
                FirstReadTime = r?.FirstReadTime,
                IsConfirmed = r != null && r.IsConfirmed && r.ConfirmedVersion >= announcement.Version,
                ConfirmedTime = r?.ConfirmedTime
            };
        }).ToList();

        return new PagedResultDto<SessionAnnouncementMemberDto>(totalCount, list);
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        var announcement = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(Guid.Empty, announcement.SessionId);
        await CheckAnyOrFallbackAsync(sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update);
        var queryable = await ReceiptRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.AnnouncementId == id && x.IsConfirmed && x.ConfirmedVersion >= announcement.Version
            && x.SessionUnit.SessionId == announcement.SessionId
            && x.SessionUnit.Setting.IsEnabled && !x.SessionUnit.Setting.IsKilled);

        var totalCount = await queryable.CountAsync();

        var receipts = await queryable
            .OrderByDescending(x => x.ConfirmedTime)
            .ThenByDescending(x => x.Id)
            .PageBy(input)
            .ToListAsync();

        var unitIds = receipts.Select(x => x.SessionUnitId).ToList();
        var units = await (await SessionUnitRepository.GetQueryableAsync()).Where(x => unitIds.Contains(x.Id))
            .Include(x => x.Owner).Include(x => x.Setting).ToDictionaryAsync(x => x.Id);
        var list = new List<SessionAnnouncementReceiptDto>();
        foreach (var r in receipts)
        {
            units.TryGetValue(r.SessionUnitId, out var unit);
            list.Add(new SessionAnnouncementReceiptDto
            {
                Id = r.Id,
                AnnouncementId = r.AnnouncementId,
                SessionUnitId = r.SessionUnitId,
                ChatObjectId = r.ChatObjectId,
                MemberName = unit?.DisplayName ?? unit?.Owner?.Name,
                MemberPortrait = unit?.Owner?.Portrait,
                FirstReadTime = r.FirstReadTime,
                IsConfirmed = r.IsConfirmed,
                ConfirmedTime = r.ConfirmedTime
            });
        }

        return new PagedResultDto<SessionAnnouncementReceiptDto>(totalCount, list);
    }

    protected virtual async Task SendAnnouncementNotificationMessageAsync(
        SessionUnit sessionUnit,
        SessionAnnouncement announcement,
        bool isUpdated)
    {
        var unitCache = ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(sessionUnit);
        var publisherName = !unitCache.MemberName.IsNullOrWhiteSpace()
            ? unitCache.MemberName
            : unitCache.OwnerName ?? sessionUnit.DisplayName ?? sessionUnit.Owner?.Name ?? string.Empty;
        var actionText = isUpdated ? "更新了群公告" : "发布了新公告";
        var announcementUrl = SessionTextTemplate.BuildUri(
            "announcement",
            new Dictionary<string, string> { ["id"] = announcement.Id.ToString() });
        var content = new CmdContentInfo
        {
            Cmd = "SessionAnnouncement",
            Text = new TextTemplate("{publisher} {actionText}：《{announcement}》")
                .WithData("publisher", SessionTextTemplate.SessionUnit(unitCache.Id, publisherName))
                .WithData("actionText", actionText)
                .WithData("announcement", SessionTextTemplate.Announcement(announcement.Id, announcement.Title))
                .ToString(),
            Url = announcementUrl
        };

        // Each update is a distinct notification; reusing the announcement ID makes
        // clients deduplicate later notifications as if they were the original one.
        var clientMessageId = LazyServiceProvider.LazyGetRequiredService<IUlidGenerator>().Generate();

        var message = await MessageManager.CreateAnnouncementCommandInCurrentTransactionAsync(
            unitCache, content, clientMessageId);
        announcement.SetLinkedMessageId(message.Id);
    }

    protected virtual async Task SendAnnouncementRemindMessageAsync(
        SessionUnit sessionUnit,
        SessionAnnouncement announcement,
        int unconfirmedCount)
    {
        var unitCache = ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(sessionUnit);
        var announcementUrl = SessionTextTemplate.BuildUri(
            "announcement",
            new Dictionary<string, string> { ["id"] = announcement.Id.ToString() });
        var statUrl = SessionTextTemplate.BuildUri(
            "announcement-stat",
            new Dictionary<string, string>
            {
                ["id"] = announcement.Id.ToString(),
                ["tab"] = "unconfirmed"
            });
        var content = new CmdContentInfo
        {
            Cmd = "SessionAnnouncement",
            Text = new TextTemplate("公告《{announcement}》还有 {count} 位成员待确认，请及时查看确认（{stat}）")
                .WithData("announcement", SessionTextTemplate.Announcement(announcement.Id, announcement.Title))
                .WithData("count", unconfirmedCount.ToString())
                .WithData("stat", $"<a href=\"{statUrl}\">待确认名单</a>")
                .ToString(),
            Url = announcementUrl
        };

        var clientMessageId = LazyServiceProvider.LazyGetRequiredService<IUlidGenerator>().Generate();
        await MessageManager.CreateAnnouncementCommandInCurrentTransactionAsync(
            unitCache, content, clientMessageId);
    }

    protected virtual async Task<SessionAnnouncementDto> MapToDtoAsync(SessionAnnouncement entity, SessionUnit currentUnit = null)
    {
        var lookupIds = new List<Guid> { entity.PublisherSessionUnitId };
        if (entity.ReviewerSessionUnitId.HasValue && entity.ReviewerSessionUnitId.Value != entity.PublisherSessionUnitId)
        {
            lookupIds.Add(entity.ReviewerSessionUnitId.Value);
        }
        var units = await (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Owner).Include(x => x.Setting)
            .Where(x => lookupIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);
        units.TryGetValue(entity.PublisherSessionUnitId, out var publisherUnit);
        SessionUnit reviewerUnit = null;
        if (entity.ReviewerSessionUnitId.HasValue)
        {
            units.TryGetValue(entity.ReviewerSessionUnitId.Value, out reviewerUnit);
        }

        var unitJoinTime = currentUnit != null ? (currentUnit.Setting?.JoinTime ?? currentUnit.CreationTime) : (DateTime?)null;
        var dto = MapToDto(entity, publisherUnit, reviewerUnit, unitJoinTime, Clock.Now);
        if (currentUnit != null)
        {
            var perms = await GetUnitPermSnapshotAsync(currentUnit);
            ApplyCapabilities(dto, entity, currentUnit, perms, dto.IsExpired);
        }
        var receipt = await GetCurrentReceiptAsync(entity.Id);
        if (receipt != null)
        {
            dto.ConfirmedVersion = receipt.ConfirmedVersion;
            dto.IsConfirmedByCurrentUser = receipt.IsConfirmed
                && receipt.ConfirmedVersion >= entity.Version
                && (!entity.ApplyToNewMembers || !unitJoinTime.HasValue || !receipt.ConfirmedTime.HasValue || receipt.ConfirmedTime.Value >= unitJoinTime.Value);
        }
        return dto;
    }

    protected virtual SessionAnnouncementDto MapToDto(
        SessionAnnouncement entity,
        SessionUnit publisherUnit,
        SessionUnit reviewerUnit = null,
        DateTime? currentUnitJoinTime = null,
        DateTime? now = null)
    {
        var current = now ?? Clock.Now;
        var isExpired = entity.ExpireTime.HasValue && entity.ExpireTime.Value <= current;
        var isEffective = !isExpired && (entity.ApplyToNewMembers || !currentUnitJoinTime.HasValue || currentUnitJoinTime.Value <= entity.CreationTime);
        return new SessionAnnouncementDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            ChatObjectId = entity.ChatObjectId,
            PublisherSessionUnitId = entity.PublisherSessionUnitId,
            PublisherChatObjectId = entity.PublisherChatObjectId,
            PublisherName = publisherUnit?.DisplayName ?? publisherUnit?.Owner?.Name,
            PublisherPortrait = publisherUnit?.Owner?.Portrait,
            Title = entity.Title,
            EditorType = entity.EditorType,
            Status = entity.Status,
            IsPinned = entity.IsPinned || entity.Sorting > 0,
            Sorting = entity.Sorting,
            PinnedTime = entity.PinnedTime,
            ExpireTime = entity.ExpireTime,
            PublishTime = entity.PublishTime,
            ApplyToNewMembers = entity.ApplyToNewMembers,
            IsExpired = isExpired,
            IsEffectiveForCurrentUser = isEffective,
            IsPopup = entity.IsPopup,
            RequireConfirm = entity.RequireConfirm,
            ConfirmedCount = entity.ConfirmedCount,
            ReadCount = entity.ReadCount,
            CreationTime = entity.CreationTime,
            Version = entity.Version,
            IsApproved = entity.IsApproved,
            ReviewerSessionUnitId = entity.ReviewerSessionUnitId,
            ReviewerChatObjectId = entity.ReviewerChatObjectId,
            ReviewerName = reviewerUnit?.DisplayName ?? reviewerUnit?.Owner?.Name,
            ReviewerPortrait = reviewerUnit?.Owner?.Portrait,
            ReviewedTime = entity.ReviewedTime,
            ReviewRemark = entity.ReviewRemark
        };
    }

    protected virtual async Task<SessionAnnouncementReceipt> GetCurrentReceiptAsync(Guid announcementId)
    {
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id!.Value);
        return await (await ReceiptRepository.GetQueryableAsync())
            .Where(x => x.AnnouncementId == announcementId && ownedIds.Contains(x.ChatObjectId))
            .OrderByDescending(x => x.IsConfirmed).ThenByDescending(x => x.ConfirmedTime).FirstOrDefaultAsync();
    }

    protected virtual async Task<SessionAnnouncementDetailDto> MapToDetailDtoAsync(SessionAnnouncement entity, SessionUnit currentUnit = null)
    {
        var dto = await MapToDtoAsync(entity, currentUnit);

        return new SessionAnnouncementDetailDto
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            ChatObjectId = dto.ChatObjectId,
            PublisherSessionUnitId = dto.PublisherSessionUnitId,
            PublisherChatObjectId = dto.PublisherChatObjectId,
            PublisherName = dto.PublisherName,
            PublisherPortrait = dto.PublisherPortrait,
            Title = dto.Title,
            EditorType = dto.EditorType,
            Status = dto.Status,
            IsPinned = dto.IsPinned,
            Sorting = dto.Sorting,
            PinnedTime = dto.PinnedTime,
            ExpireTime = dto.ExpireTime,
            PublishTime = dto.PublishTime,
            ApplyToNewMembers = dto.ApplyToNewMembers,
            IsExpired = dto.IsExpired,
            IsEffectiveForCurrentUser = dto.IsEffectiveForCurrentUser,
            IsPopup = dto.IsPopup,
            RequireConfirm = dto.RequireConfirm,
            ConfirmedCount = dto.ConfirmedCount,
            ReadCount = dto.ReadCount,
            CreationTime = dto.CreationTime,
            IsConfirmedByCurrentUser = dto.IsConfirmedByCurrentUser,
            ConfirmedTime = (await GetCurrentReceiptAsync(entity.Id))?.ConfirmedTime,
            Content = entity.Content,
            LinkedMessageId = entity.LinkedMessageId,
            Version = dto.Version,
            IsApproved = dto.IsApproved,
            ReviewerSessionUnitId = dto.ReviewerSessionUnitId,
            ReviewerChatObjectId = dto.ReviewerChatObjectId,
            ReviewerName = dto.ReviewerName,
            ReviewerPortrait = dto.ReviewerPortrait,
            ReviewedTime = dto.ReviewedTime,
            ReviewRemark = dto.ReviewRemark,
            ConfirmedVersion = dto.ConfirmedVersion,
            CanEdit = dto.CanEdit,
            CanDelete = dto.CanDelete,
            CanPublish = dto.CanPublish,
            CanSubmitReview = dto.CanSubmitReview,
            CanApprove = dto.CanApprove,
            CanRevoke = dto.CanRevoke,
            CanPin = dto.CanPin,
            CanRemind = dto.CanRemind,
            CanViewStat = dto.CanViewStat
        };
    }
}
