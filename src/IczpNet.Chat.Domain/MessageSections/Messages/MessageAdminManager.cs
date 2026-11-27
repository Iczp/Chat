using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageAdminManager(
    IMessageRepository messageRepository,
    IMessageManager messageManager,
    IMessageSender messageSender,
    ISessionUnitManager sessionUnitManager) : DomainService, IMessageAdminManager, ITransientDependency
{
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IMessageManager MessageManager { get; } = messageManager;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;

    public virtual async Task<MessageAdminHistoryResult> GetHistoryAdminAsync(
        Guid sessionUnitId,
        long? maxMessageId,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit == null)
        {
            throw new UserFriendlyException("会话单元不存在。");
        }

        var pageSize = Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 100);
        var messageIdList = await MessageManager.GetHistoryAsync(unit, maxMessageId, pageSize + 1);

        var hasMore = messageIdList.Count > pageSize;
        long? nextCursorId = null;
        if (hasMore)
        {
            messageIdList.RemoveAt(messageIdList.Count - 1);
            nextCursorId = messageIdList.LastOrDefault();
        }

        var items = new List<MessageCacheItem>();
        if (messageIdList.Count > 0)
        {
            var cached = await MessageManager.GetOrAddManyCacheAsync(messageIdList, token: cancellationToken);
            items = cached.Select(x => x.Value).Where(x => x != null).ToList();
        }

        return new MessageAdminHistoryResult
        {
            TotalCount = items.Count + (hasMore ? 1 : 0),
            Items = items,
            HasMore = hasMore,
            NextCursorId = nextCursorId
        };
    }

    public virtual async Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<ArticleContentInput> input,
        CancellationToken cancellationToken = default)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit == null || !unit.IsEnabled)
        {
            throw new UserFriendlyException("会话单元不存在或已停用。");
        }

        return await MessageSender.SendArticleContentMessageAsync(unit, input);
    }

    public virtual async Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        CancellationToken cancellationToken = default)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit == null || !unit.IsEnabled)
        {
            throw new UserFriendlyException("会话单元不存在或已停用。");
        }

        return await MessageSender.SendTemplateContentMessageAsync(unit, input);
    }

    public virtual async Task<Dictionary<string, long>> RollbackAdminAsync(
        long messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await MessageRepository.GetAsync(messageId, cancellationToken: cancellationToken);
        return await MessageManager.RollbackAsync(message);
    }
}
