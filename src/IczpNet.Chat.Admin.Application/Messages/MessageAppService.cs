using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Messages.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Admin.Messages;

/// <summary>
/// 管理端消息发送与历史服务
/// </summary>
[Authorize(ChatAdminPermissions.Messages.Default)]
[ExposeServices(typeof(MessageAppService), typeof(IMessageAdminAppService))]
public class MessageAppService(
    IMessageAdminManager messageAdminManager,
    IChatObjectManager chatObjectManager,
    ISessionUnitRepository sessionUnitRepository)
    : ApplicationService, IMessageAdminAppService
{
    protected IMessageAdminManager MessageAdminManager { get; } = messageAdminManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;

    protected virtual async Task<MessageAdminScope> BuildScopeAsync(CancellationToken cancellationToken = default)
    {
        var hasManageAll = !CurrentUser.Id.HasValue
            || await AuthorizationService.IsGrantedAsync(ChatAdminPermissions.Messages.ManageAll);
        if (hasManageAll)
        {
            return new MessageAdminScope
            {
                TenantId = CurrentTenant.Id,
                HasManageAll = true
            };
        }

        var ownerIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id!.Value);
        if (ownerIds == null || ownerIds.Count == 0)
        {
            return new MessageAdminScope
            {
                TenantId = CurrentTenant.Id,
                HasManageAll = false
            };
        }

        var sessionIds = await AsyncExecuter.ToListAsync(
            (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => ownerIds.Contains(x.OwnerId) && x.SessionId.HasValue)
                .Select(x => x.SessionId!.Value)
                .Distinct(),
            cancellationToken);

        return new MessageAdminScope
        {
            TenantId = CurrentTenant.Id,
            HasManageAll = false,
            AllowedOwnerIds = ownerIds,
            AllowedSessionIds = sessionIds
        };
    }

    [Authorize(ChatAdminPermissions.Messages.GetList)]
    public virtual async Task<PagedResultDto<MessageAdminListItemDto>> GetListAsync(
        MessageAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new MessageAdminGetListInput();
        var scope = await BuildScopeAsync(cancellationToken);

        var result = await MessageAdminManager.GetAdminListAsync(
            input.SessionId,
            input.SenderId,
            input.ReceiverId,
            input.MessageType,
            input.IsRollbacked,
            input.IsPrivate,
            input.MaxMessageId,
            input.StartTime,
            input.EndTime,
            input.SkipCount,
            input.MaxResultCount,
            scope,
            cancellationToken);

        return new PagedResultDto<MessageAdminListItemDto>(
            result.TotalCount,
            result.Items.Select(MapToListItemDto).ToList());
    }

    [Authorize(ChatAdminPermissions.Messages.GetDetail)]
    public virtual async Task<MessageAdminDetailDto> GetAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(cancellationToken);
        var entity = await MessageAdminManager.GetAdminDetailAsync(id, scope, cancellationToken);
        var canViewContent = await AuthorizationService.IsGrantedAsync(ChatAdminPermissions.Messages.GetContent);
        return new MessageAdminDetailDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            SessionSeq = entity.SessionSeq,
            SenderId = entity.SenderId,
            SenderName = entity.Sender?.Name,
            ReceiverId = entity.ReceiverId,
            ReceiverName = entity.Receiver?.Name,
            Channel = entity.Channel,
            MessageType = entity.MessageType,
            IsPrivate = entity.IsPrivate,
            IsRollbacked = entity.IsRollbacked,
            RollbackTime = entity.RollbackTime,
            Size = entity.Size,
            CreationTime = entity.CreationTime,
            ClientMessageId = entity.ClientMessageId,
            ContentJson = canViewContent ? entity.ContentJson : null,
            SenderSessionUnitId = entity.SenderSessionUnitId,
            ReceiverSessionUnitId = entity.ReceiverSessionUnitId
        };
    }

    private static MessageAdminListItemDto MapToListItemDto(Message entity)
    {
        return new MessageAdminListItemDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            SessionSeq = entity.SessionSeq,
            SenderId = entity.SenderId,
            SenderName = entity.Sender?.Name,
            ReceiverId = entity.ReceiverId,
            ReceiverName = entity.Receiver?.Name,
            Channel = entity.Channel,
            MessageType = entity.MessageType,
            IsPrivate = entity.IsPrivate,
            IsRollbacked = entity.IsRollbacked,
            RollbackTime = entity.RollbackTime,
            Size = entity.Size,
            CreationTime = entity.CreationTime
        };
    }

    [Authorize(ChatAdminPermissions.Messages.GetList)]
    public virtual async Task<MessageAdminHistoryResultDto> GetHistoryAsync(
        MessageAdminGetHistoryInput input,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(cancellationToken);
        var result = await MessageAdminManager.GetHistoryAdminAsync(
            input.SessionUnitId,
            input.MaxMessageId,
            input.MaxResultCount,
            scope,
            cancellationToken);
        var canViewContent = await AuthorizationService.IsGrantedAsync(ChatAdminPermissions.Messages.GetContent);

        var items = result.Items.Select(x => new MessageAdminHistoryItemDto
        {
            Id = x.Id,
            SessionId = x.SessionId,
            SenderId = x.SenderId,
            SenderName = x.SenderName,
            SenderDisplayName = x.SenderDisplayName,
            SenderSessionUnitId = x.SenderSessionUnitId,
            MessageType = x.MessageType,
            MessageTypeDescription = x.MessageTypeDescription,
            IsRollbacked = x.IsRollbacked,
            RollbackTime = x.RollbackTime,
            CreationTime = x.CreationTime,
            Content = canViewContent ? x.Content : null
        }).ToList();

        return new MessageAdminHistoryResultDto(
            result.TotalCount,
            items,
            new MessageAdminHistoryExtraDto
            {
                HasMore = result.HasMore,
                NextCursorId = result.NextCursorId
            });
    }

    [Authorize(ChatAdminPermissions.Messages.Send)]
    public virtual async Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(
        Guid sessionUnitId,
        MessageInput<ArticleContentInput> input,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(cancellationToken);
        return await MessageAdminManager.SendArticleContentMessageAdminAsync(sessionUnitId, input, scope, cancellationToken);
    }

    [Authorize(ChatAdminPermissions.Messages.Send)]
    public virtual async Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(cancellationToken);
        return await MessageAdminManager.SendTemplateContentMessageAdminAsync(sessionUnitId, input, scope, cancellationToken);
    }

    [UnitOfWork(isTransactional: true)]
    [Authorize(ChatAdminPermissions.Messages.Rollback)]
    public virtual async Task<Dictionary<string, long>> RollbackAsync(
        long messageId,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(cancellationToken);
        return await MessageAdminManager.RollbackAdminAsync(messageId, scope, cancellationToken);
    }
}
