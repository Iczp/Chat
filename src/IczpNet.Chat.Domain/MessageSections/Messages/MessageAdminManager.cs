using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
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

    private static bool IsScopeRestricted(MessageAdminScope scope)
        => scope != null && !scope.HasManageAll;

    private static bool HasMessageAccess(Message message, MessageAdminScope scope)
    {
        if (!IsScopeRestricted(scope))
        {
            return true;
        }

        if (message.SessionId.HasValue && scope.AllowedSessionIds.Contains(message.SessionId.Value))
        {
            return true;
        }

        if (message.SenderId.HasValue && scope.AllowedOwnerIds.Contains(message.SenderId.Value))
        {
            return true;
        }

        if (message.ReceiverId.HasValue && scope.AllowedOwnerIds.Contains(message.ReceiverId.Value))
        {
            return true;
        }

        return false;
    }

    public virtual async Task<MessageAdminListResult> GetAdminListAsync(
        Guid? sessionId,
        long? senderId,
        long? receiverId,
        MessageTypes? messageType,
        bool? isRollbacked,
        bool? isPrivate,
        long? maxMessageId,
        DateTime? startTime,
        DateTime? endTime,
        int skipCount,
        int maxResultCount,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        if (IsScopeRestricted(scope) && scope.AllowedOwnerIds.Count == 0 && scope.AllowedSessionIds.Count == 0)
        {
            return new MessageAdminListResult();
        }

        var query = (await MessageRepository.GetQueryableAsync())
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .WhereIf(sessionId.HasValue, x => x.SessionId == sessionId.Value)
            .WhereIf(senderId.HasValue, x => x.SenderId == senderId.Value)
            .WhereIf(receiverId.HasValue, x => x.ReceiverId == receiverId.Value)
            .WhereIf(messageType.HasValue, x => x.MessageType == messageType.Value)
            .WhereIf(isRollbacked.HasValue, x => x.IsRollbacked == isRollbacked.Value)
            .WhereIf(isPrivate.HasValue, x => x.IsPrivate == isPrivate.Value)
            .WhereIf(maxMessageId.HasValue, x => x.Id < maxMessageId.Value)
            .WhereIf(startTime.HasValue, x => x.CreationTime >= startTime.Value)
            .WhereIf(endTime.HasValue, x => x.CreationTime < endTime.Value);

        if (IsScopeRestricted(scope))
        {
            var ownerIds = scope.AllowedOwnerIds;
            var sessionIds = scope.AllowedSessionIds;
            query = query.Where(x =>
                (x.SessionId.HasValue && sessionIds.Contains(x.SessionId.Value)) ||
                (x.SenderId.HasValue && ownerIds.Contains(x.SenderId.Value)) ||
                (x.ReceiverId.HasValue && ownerIds.Contains(x.ReceiverId.Value)));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new MessageAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.Id)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 200)),
            cancellationToken);

        return new MessageAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<Message> GetAdminDetailAsync(
        long id,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        var query = (await MessageRepository.GetQueryableAsync())
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x => x.Id == id);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Message), id);
        }

        if (!HasMessageAccess(entity, scope))
        {
            throw new AbpAuthorizationException("无权访问该消息。");
        }

        return entity;
    }

    public virtual async Task<MessageAdminHistoryResult> GetHistoryAdminAsync(
        Guid sessionUnitId,
        long? maxMessageId,
        int maxResultCount,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit == null)
        {
            throw new UserFriendlyException("会话单元不存在。");
        }

        if (IsScopeRestricted(scope) &&
            !scope.AllowedOwnerIds.Contains(unit.OwnerId) &&
            (!unit.SessionId.HasValue || !scope.AllowedSessionIds.Contains(unit.SessionId.Value)))
        {
            throw new AbpAuthorizationException("无权访问该会话历史。");
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
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit == null || !unit.IsEnabled)
        {
            throw new UserFriendlyException("会话单元不存在或已停用。");
        }

        if (IsScopeRestricted(scope) && !scope.AllowedOwnerIds.Contains(unit.OwnerId))
        {
            throw new AbpAuthorizationException("无权以该身份发送消息。");
        }

        return await MessageSender.SendArticleContentMessageAsync(unit, input);
    }

    public virtual async Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit == null || !unit.IsEnabled)
        {
            throw new UserFriendlyException("会话单元不存在或已停用。");
        }

        if (IsScopeRestricted(scope) && !scope.AllowedOwnerIds.Contains(unit.OwnerId))
        {
            throw new AbpAuthorizationException("无权以该身份发送消息。");
        }

        return await MessageSender.SendTemplateContentMessageAsync(unit, input);
    }

    public virtual async Task<Dictionary<string, long>> RollbackAdminAsync(
        long messageId,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        var message = await MessageRepository.GetAsync(messageId, cancellationToken: cancellationToken);
        if (IsScopeRestricted(scope) &&
            !(message.SenderId.HasValue && scope.AllowedOwnerIds.Contains(message.SenderId.Value)) &&
            !(message.SessionId.HasValue && scope.AllowedSessionIds.Contains(message.SessionId.Value)))
        {
            throw new AbpAuthorizationException("无权撤回该消息。");
        }

        return await MessageManager.RollbackAsync(message);
    }
}
