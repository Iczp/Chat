using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionSections.Announcements.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
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
        if (sessionUnitId != Guid.Empty)
        {
            var unit = await SessionUnitRepository.FindAsync(sessionUnitId);
            if (unit != null)
            {
                return unit;
            }
        }
        if (CurrentUser.Id.HasValue && sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            var candidates = await SessionUnitRepository.GetListAsync(x => x.SessionId == sessionId.Value && ownedIds.Contains(x.OwnerId) && x.Setting.IsEnabled && !x.Setting.IsKilled);
            var found = candidates.FirstOrDefault(x => x.Setting.IsCreator) ?? candidates.FirstOrDefault(x => x.Setting.IsAdmin) ?? candidates.FirstOrDefault();
            if (found != null)
            {
                return found;
            }
        }
        return sessionUnitId != Guid.Empty ? await SessionUnitManager.GetAsync(sessionUnitId) : null;
    }

    [HttpGet]
    public virtual async Task<SessionAnnouncementDetailDto> GetPinnedAsync(Guid sessionId)
    {
        var entity = await AnnouncementRepository.GetPinnedAsync(sessionId);
        if (entity == null)
        {
            return null;
        }

        return await MapToDetailDtoAsync(entity);
    }

    [HttpGet]
    public virtual async Task<SessionAnnouncementDetailDto> GetAsync(Guid id)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        return await MapToDetailDtoAsync(entity);
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input)
    {
        var queryable = await AnnouncementRepository.GetQueryableAsync();

        queryable = queryable.Where(x => x.SessionId == input.SessionId);

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
            .PageBy(input)
            .ToListAsync();

        var dtoList = new List<SessionAnnouncementDto>();
        foreach (var entity in entities)
        {
            dtoList.Add(await MapToDtoAsync(entity));
        }

        return new PagedResultDto<SessionAnnouncementDto>(totalCount, dtoList);
    }

    [HttpPost]
    public virtual async Task<SessionAnnouncementDetailDto> CreateAsync(CreateSessionAnnouncementInput input)
    {
        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, input.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Create, sessionUnit);

        var sessionId = input.SessionId ?? sessionUnit.SessionId!.Value;
        var chatObjectId = sessionUnit.DestinationId ?? sessionUnit.OwnerId;

        using var uow = UnitOfWorkManager.Begin(requiresNew: false, isTransactional: true);

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

        using var uow = UnitOfWorkManager.Begin(requiresNew: false, isTransactional: true);

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

        await AnnouncementManager.SetPinnedAsync(entity, input.IsPinned);
    }

    [HttpPost]
    public virtual async Task DeleteAsync(Guid id, Guid sessionUnitId)
    {
        var entity = await AnnouncementRepository.GetAsync(id);
        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, entity.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Delete, sessionUnit);

        await AnnouncementManager.DeleteAsync(entity);
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

    [HttpGet]
    public virtual async Task<SessionAnnouncementStatDto> GetStatAsync(Guid id, Guid sessionUnitId)
    {
        var announcement = await AnnouncementRepository.GetAsync(id);
        Check.NotNull(announcement.SessionId, nameof(announcement.SessionId));

        var sessionUnit = await GetSessionUnitOrDefaultAsync(sessionUnitId, announcement.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        var totalMembers = await SessionUnitManager.GetCountBySessionIdAsync(announcement.SessionId.Value);

        var receipts = await ReceiptRepository.GetQueryableAsync();
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

    [HttpGet]
    public virtual async Task<PagedResultDto<SessionAnnouncementMemberDto>> GetMembersAsync(Guid id, GetAnnouncementMemberListInput input)
    {
        var announcement = await AnnouncementRepository.GetAsync(id);
        Check.NotNull(announcement.SessionId, nameof(announcement.SessionId));

        var sessionUnit = await GetSessionUnitOrDefaultAsync(input.SessionUnitId, announcement.SessionId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        var receiptsQuery = (await ReceiptRepository.GetQueryableAsync()).Where(x => x.AnnouncementId == id);
        var unitQuery = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == announcement.SessionId && x.IsEnabled);

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            unitQuery = unitQuery.Where(x => (x.DisplayName != null && x.DisplayName.Contains(keyword)) ||
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
            .PageBy(input)
            .Include(x => x.Owner)
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

    [HttpGet]
    public virtual async Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        var queryable = await ReceiptRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.AnnouncementId == id && x.IsConfirmed);

        var totalCount = await queryable.CountAsync();

        var receipts = await queryable
            .OrderByDescending(x => x.ConfirmedTime)
            .PageBy(input)
            .ToListAsync();

        var list = new List<SessionAnnouncementReceiptDto>();
        foreach (var r in receipts)
        {
            var unit = await SessionUnitManager.GetAsync(r.SessionUnitId);
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

        var clientMessageId = "AN" + Convert.ToBase64String(announcement.Id.ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        if (clientMessageId.Length > 26) clientMessageId = clientMessageId[..26];

        var message = await MessageManager.CreateAnnouncementCommandInCurrentTransactionAsync(
            unitCache, content, clientMessageId);
        announcement.SetLinkedMessageId(message.Id);
    }

    protected virtual async Task<SessionAnnouncementDto> MapToDtoAsync(SessionAnnouncement entity)
    {
        var publisherUnit = await SessionUnitManager.GetAsync(entity.PublisherSessionUnitId);

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
            Content = entity.Content,
            LinkedMessageId = entity.LinkedMessageId
        };
    }
}
