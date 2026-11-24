using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionSections.Announcements.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Ulids;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
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
    IMessageManager messageManager) : ChatAppService, ISessionAnnouncementAppService
{
    protected ISessionAnnouncementRepository AnnouncementRepository { get; } = announcementRepository;
    protected ISessionAnnouncementReceiptRepository ReceiptRepository { get; } = receiptRepository;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected ISessionAnnouncementManager AnnouncementManager { get; } = announcementManager;
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IMessageManager MessageManager { get; } = messageManager;

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

        return await MapToDetailDtoAsync(entity);
    }

    public virtual async Task<SessionAnnouncementDetailDto> GetAsync(Guid id)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, entity.SessionId);
        if (entity.Status != AnnouncementStatus.Published)
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, unit);
        return await MapToDetailDtoAsync(entity);
    }

    public virtual async Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input)
    {
        var unit = await GetSessionUnitOrDefaultAsync(Guid.Empty, input.SessionId);
        var targetSessionId = unit.SessionId!.Value;
        var queryable = await AnnouncementRepository.GetQueryableAsync();

        queryable = queryable.Where(x => x.SessionId == targetSessionId && x.Status == AnnouncementStatus.Published);

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
            .OrderByDescending(x => x.IsPinned)
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
        var confirmedIds = await (await ReceiptRepository.GetQueryableAsync())
            .Where(x => ids.Contains(x.AnnouncementId) && ownedIds.Contains(x.ChatObjectId) && x.IsConfirmed)
            .Select(x => x.AnnouncementId).Distinct().ToListAsync();
        foreach (var entity in entities)
        {
            publishers.TryGetValue(entity.PublisherSessionUnitId, out var publisher);
            var dto = MapToDto(entity, publisher);
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

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Create, sessionUnit);
        if (input.IsPinned)
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        var sessionId = sessionUnit.SessionId!.Value;
        var chatObjectId = sessionUnit.DestinationId ?? sessionUnit.OwnerId;

        await using var handle = await LockSessionAsync(sessionId);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

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
            requireConfirm: input.RequireConfirm);

        if (input.SendMessageNotification)
        {
            await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: false);
        }

        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity);
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
        if (input.IsPinned != entity.IsPinned)
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        entity = await AnnouncementManager.UpdateAsync(
            announcement: entity,
            title: input.Title,
            content: input.Content,
            editorType: input.EditorType,
            isPinned: input.IsPinned,
            isPopup: input.IsPopup,
            requireConfirm: input.RequireConfirm);

        if (input.SendMessageNotification)
        {
            await SendAnnouncementNotificationMessageAsync(sessionUnit, entity, isUpdated: true);
        }

        await uow.CompleteAsync();

        return await MapToDetailDtoAsync(entity);
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
        await AnnouncementManager.SetPinnedAsync(entity, input.IsPinned);
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
            .Where(x => x.SessionId == announcement.SessionId && x.Setting.IsEnabled && !x.Setting.IsKilled);
        var totalMembers = await members.CountAsync();

        var receipts = (await ReceiptRepository.GetQueryableAsync()).Where(x => members.Any(u => u.Id == x.SessionUnitId));
        var readCount = await receipts.CountAsync(x => x.AnnouncementId == id);
        var confirmedCount = await receipts.CountAsync(x => x.AnnouncementId == id && x.IsConfirmed);

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

        var pagedUnits = await unitQuery
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.Id)
            .PageBy(input)
            .Include(x => x.Owner)
            .Include(x => x.Setting)
            .ToListAsync();

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
        var actionText = isUpdated ? "更新了群公告" : "发布了新公告";
        var content = new CmdContentInfo
        {
            Cmd = "SessionAnnouncement",
            Text = $"{sessionUnit.DisplayName ?? sessionUnit.Owner?.Name} {actionText}：《{announcement.Title}》",
            Url = $"app:///pages/im/announcement?id={announcement.Id}"
        };

        // Each update is a distinct notification; reusing the announcement ID makes
        // clients deduplicate later notifications as if they were the original one.
        var clientMessageId = LazyServiceProvider.LazyGetRequiredService<IUlidGenerator>().Generate();

        var message = await MessageManager.CreateAnnouncementCommandInCurrentTransactionAsync(
            unitCache, content, clientMessageId);
        announcement.SetLinkedMessageId(message.Id);
    }

    protected virtual async Task<SessionAnnouncementDto> MapToDtoAsync(SessionAnnouncement entity)
    {
        var publisherUnit = await (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Owner).Include(x => x.Setting).FirstOrDefaultAsync(x => x.Id == entity.PublisherSessionUnitId);
        var dto = MapToDto(entity, publisherUnit);
        var receipt = await GetCurrentReceiptAsync(entity.Id);
        dto.IsConfirmedByCurrentUser = receipt?.IsConfirmed ?? false;
        return dto;
    }

    protected virtual SessionAnnouncementDto MapToDto(SessionAnnouncement entity, SessionUnit publisherUnit)
    {
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
            IsPinned = entity.IsPinned,
            PinnedTime = entity.PinnedTime,
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

    protected virtual async Task<SessionAnnouncementDetailDto> MapToDetailDtoAsync(SessionAnnouncement entity)
    {
        var dto = await MapToDtoAsync(entity);

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
            PinnedTime = dto.PinnedTime,
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
