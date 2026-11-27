using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Messages.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Admin.Messages;

/// <summary>
/// 管理端消息发送与历史服务
/// </summary>
[Authorize(ChatAdminPermissions.Messages.Default)]
[ExposeServices(typeof(MessageAppService), typeof(IMessageAdminAppService))]
public class MessageAppService(IMessageAdminManager messageAdminManager)
    : ApplicationService, IMessageAdminAppService
{
    protected IMessageAdminManager MessageAdminManager { get; } = messageAdminManager;

    [Authorize(ChatAdminPermissions.Messages.GetList)]
    public virtual async Task<MessageAdminHistoryResultDto> GetHistoryAsync(
        MessageAdminGetHistoryInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await MessageAdminManager.GetHistoryAdminAsync(
            input.SessionUnitId,
            input.MaxMessageId,
            input.MaxResultCount,
            cancellationToken);

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
            Content = x.Content
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
    public virtual Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(
        Guid sessionUnitId,
        MessageInput<ArticleContentInput> input,
        CancellationToken cancellationToken = default)
    {
        return MessageAdminManager.SendArticleContentMessageAdminAsync(sessionUnitId, input, cancellationToken);
    }

    [Authorize(ChatAdminPermissions.Messages.Send)]
    public virtual Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        CancellationToken cancellationToken = default)
    {
        return MessageAdminManager.SendTemplateContentMessageAdminAsync(sessionUnitId, input, cancellationToken);
    }

    [UnitOfWork(isTransactional: true)]
    [Authorize(ChatAdminPermissions.Messages.Rollback)]
    public virtual Task<Dictionary<string, long>> RollbackAsync(
        long messageId,
        CancellationToken cancellationToken = default)
    {
        return MessageAdminManager.RollbackAdminAsync(messageId, cancellationToken);
    }
}
