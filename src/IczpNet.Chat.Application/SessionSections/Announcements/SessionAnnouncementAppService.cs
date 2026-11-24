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
    IDistributedCache<string> distributedCache) : ChatAppService, ISessionAnnouncementAppService
{
    protected ISessionAnnouncementRepository AnnouncementRepository { get; } = announcementRepository;
    protected ISessionAnnouncementReceiptRepository ReceiptRepository { get; } = receiptRepository;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected ISessionAnnouncementManager AnnouncementManager { get; } = announcementManager;
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IMessageManager MessageManager { get; } = messageManager;
    protected IDistributedCache<string> DistributedCache { get; } = distributedCache;

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
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, unit);
        return await MapToDetailDtoAsync(entity, unit);
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input)
    {
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, input.SessionId);
        var targetSessionId = unit.SessionId!.Value;
        var queryable = await AnnouncementRepository.GetQueryableAsync();

        var allowAllStatuses = false;
        if (input.IncludeDrafts)
        {
            allowAllStatuses = await SessionPermissionChecker.IsGrantedAsync(
                SessionPermissionDefinitionConsts.AnnouncementPermission.Update,
                unit);
        }

        queryable = allowAllStatuses
            ? queryable.Where(x => x.SessionId == targetSessionId)
            : queryable.Where(x => x.SessionId == targetSessionId && x.Status == AnnouncementStatus.Published);

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
        var publisherIds = entities.Select(x => x.PublisherSessionUnitId).Distinct().ToList();
        var publishers = await (await SessionUnitRepository.GetQueryableAsync()).Where(x => publisherIds.Contains(x.Id))
            .Include(x => x.Owner).Include(x => x.Setting).ToDictionaryAsync(x => x.Id);
        var ids = entities.Select(x => x.Id).ToList();
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id!.Value);
        var confirmedReceipts = await (await ReceiptRepository.GetQueryableAsync())
            .Where(x => ids.Contains(x.AnnouncementId) && ownedIds.Contains(x.ChatObjectId) && x.IsConfirmed)
            .GroupBy(x => x.AnnouncementId)
            .Select(g => new { AnnouncementId = g.Key, LatestConfirmedTime = g.Max(x => x.ConfirmedTime) })
            .ToDictionaryAsync(x => x.AnnouncementId, x => x.LatestConfirmedTime);

        var unitJoinTime = unit.Setting?.JoinTime ?? unit.CreationTime;
        var now = Clock.Now;
        foreach (var entity in entities)
        {
            publishers.TryGetValue(entity.PublisherSessionUnitId, out var publisher);
            var dto = MapToDto(entity, publisher, unitJoinTime, now);
            if (confirmedReceipts.TryGetValue(entity.Id, out var confirmedTime))
            {
                dto.IsConfirmedByCurrentUser = !entity.ApplyToNewMembers
                    || !confirmedTime.HasValue
                    || confirmedTime.Value >= unitJoinTime;
            }
            dtoList.Add(dto);
        }

        return new PagedResultDto<SessionAnnouncementDto>(totalCount, dtoList);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> CreateAsync(CreateSessionAnnouncementInput input)
    {
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, input.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Create, sessionUnit);
        if (input.IsPinned || (input.Sorting.HasValue && input.Sorting.Value > 0))
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        var sessionId = sessionUnit.SessionId!.Value;
        var chatObjectId = sessionUnit.DestinationId ?? sessionUnit.OwnerId;

        await using var handle = await LockSessionAsync(sessionId);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        var isScheduledFuture = input.PublishTime.HasValue && input.PublishTime.Value > Clock.Now;
        var targetStatus = (input.SaveAsDraft || isScheduledFuture)
            ? AnnouncementStatus.Draft
            : AnnouncementStatus.Published;

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
            publishTime: input.PublishTime);

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

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

        await using var handle = await LockSessionAsync(entity.SessionId!.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        entity = await AnnouncementRepository.GetAsync(id);
        if (input.IsPinned != entity.IsPinned || (input.Sorting.HasValue && input.Sorting.Value != entity.Sorting))
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        var isScheduledFuture = input.PublishTime.HasValue && input.PublishTime.Value > Clock.Now;
        var nextStatus = (input.SaveAsDraft || isScheduledFuture)
            ? AnnouncementStatus.Draft
            : AnnouncementStatus.Published;

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
            resetReceipts: input.ResetReceipts);

        if (input.SendMessageNotification && entity.Status == AnnouncementStatus.Published)
        {
            await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: true);
        }

        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity, sessionUnit);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> PublishAsync(Guid id, Guid sessionUnitId)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

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

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

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
        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

        if (entity.Status != AnnouncementStatus.Published)
        {
            throw new UserFriendlyException("仅已发布公告支持发送催办提醒");
        }
        if (entity.ExpireTime.HasValue && entity.ExpireTime.Value <= Clock.Now)
        {
            throw new UserFriendlyException("该公告已过期，无法发送催办提醒");
        }

        var cooldownKey = $"Chat_Announcement_RemindCooldown:{id}";
        var lastRemind = await DistributedCache.GetAsync(cooldownKey);
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
            .Where(x => x.AnnouncementId == id && x.IsConfirmed)
            .Select(x => x.SessionUnitId);
        var unconfirmedCount = await members.CountAsync(x => !confirmedUnitIds.Contains(x.Id));

        if (unconfirmedCount <= 0)
        {
            return 0;
        }

        await SendAnnouncementRemindMessageAsync(sessionUnit, entity, unconfirmedCount);
        await DistributedCache.SetAsync(
            cooldownKey,
            Clock.Now.ToString("O"),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

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

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Delete, sessionUnit);

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
        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

        var members = (await SessionUnitRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.SessionId == announcement.SessionId && x.Setting.IsEnabled && !x.Setting.IsKilled);
        if (!announcement.ApplyToNewMembers)
        {
            members = members.Where(x => (x.Setting.JoinTime ?? x.Setting.CreationTime) <= announcement.CreationTime);
        }

        var countCacheKey = $"Chat_Announcement_MemberCount:{announcement.SessionId}:{announcement.ApplyToNewMembers}:{announcement.CreationTime.Ticks}";
        var cachedCountStr = await DistributedCache.GetAsync(countCacheKey);
        int totalMembers;
        if (int.TryParse(cachedCountStr, out var parsedCount))
        {
            totalMembers = parsedCount;
        }
        else
        {
            totalMembers = await members.CountAsync();
            await DistributedCache.SetAsync(
                countCacheKey,
                totalMembers.ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
        }

        var receiptStats = await (await ReceiptRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.AnnouncementId == id && members.Any(u => u.Id == x.SessionUnitId))
            .GroupBy(x => 1)
            .Select(g => new
            {
                ReadCount = g.Count(),
                ConfirmedCount = g.Count(x => x.IsConfirmed)
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
        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

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
                var confirmedUnitIds = receiptsQuery.Where(x => x.IsConfirmed).Select(x => x.SessionUnitId);
                unitQuery = unitQuery.Where(x => confirmedUnitIds.Contains(x.Id));
                break;
            case AnnouncementMemberFilterType.Unconfirmed:
                var alreadyConfirmedUnitIds = receiptsQuery.Where(x => x.IsConfirmed).Select(x => x.SessionUnitId);
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
                IsConfirmed = r?.IsConfirmed ?? false,
                ConfirmedTime = r?.ConfirmedTime
            };
        }).ToList();

        return new PagedResultDto<SessionAnnouncementMemberDto>(totalCount, list);
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        var announcement = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(Guid.Empty, announcement.SessionId);
        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);
        var queryable = await ReceiptRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.AnnouncementId == id && x.IsConfirmed
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
        var publisherUnit = await (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Owner).Include(x => x.Setting).FirstOrDefaultAsync(x => x.Id == entity.PublisherSessionUnitId);
        var unitJoinTime = currentUnit != null ? (currentUnit.Setting?.JoinTime ?? currentUnit.CreationTime) : (DateTime?)null;
        var dto = MapToDto(entity, publisherUnit, unitJoinTime, Clock.Now);
        var receipt = await GetCurrentReceiptAsync(entity.Id);
        dto.IsConfirmedByCurrentUser = receipt != null && receipt.IsConfirmed
            && (!entity.ApplyToNewMembers || !unitJoinTime.HasValue || !receipt.ConfirmedTime.HasValue || receipt.ConfirmedTime.Value >= unitJoinTime.Value);
        return dto;
    }

    protected virtual SessionAnnouncementDto MapToDto(
        SessionAnnouncement entity,
        SessionUnit publisherUnit,
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
            CreationTime = entity.CreationTime
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
            LinkedMessageId = entity.LinkedMessageId
        };
    }
}
