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
    ISessionAnnouncementManager announcementManager,
    ISessionPermissionChecker sessionPermissionChecker,
    IMessageManager messageManager) : ChatAppService, ISessionAnnouncementAppService
{
    protected ISessionAnnouncementRepository AnnouncementRepository { get; } = announcementRepository;
    protected ISessionAnnouncementReceiptRepository ReceiptRepository { get; } = receiptRepository;
    protected ISessionAnnouncementManager AnnouncementManager { get; } = announcementManager;
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IMessageManager MessageManager { get; } = messageManager;

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
        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
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
        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Update, sessionUnit);

        var entity = await AnnouncementRepository.GetAsync(id);

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
        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Pin, sessionUnit);

        var entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.SetPinnedAsync(entity, input.IsPinned);
    }

    [HttpPost]
    public virtual async Task DeleteAsync(Guid id, Guid sessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Delete, sessionUnit);

        var entity = await AnnouncementRepository.GetAsync(id);
        await AnnouncementManager.DeleteAsync(entity);
    }

    [HttpPost]
    public virtual async Task ConfirmReceiptAsync(Guid id, ConfirmSessionAnnouncementInput input)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
        Check.NotNull(sessionUnit, nameof(sessionUnit));

        await AnnouncementManager.ConfirmReceiptAsync(
            announcementId: id,
            sessionUnitId: sessionUnit.Id,
            chatObjectId: sessionUnit.OwnerId,
            deviceId: input.DeviceId);
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
