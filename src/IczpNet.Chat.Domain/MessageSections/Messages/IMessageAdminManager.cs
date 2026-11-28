using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageAdminListResult
{
    public long TotalCount { get; set; }

    public List<Message> Items { get; set; } = new();
}

public class MessageAdminHistoryResult
{
    public long TotalCount { get; set; }

    public List<MessageCacheItem> Items { get; set; } = new();

    public bool HasMore { get; set; }

    public long? NextCursorId { get; set; }
}

public class MessageAdminScope
{
    public Guid? TenantId { get; set; }

    public bool HasManageAll { get; set; }

    public IReadOnlyCollection<long> AllowedOwnerIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<Guid> AllowedSessionIds { get; set; } = Array.Empty<Guid>();
}

public interface IMessageAdminManager
{
    Task<MessageAdminListResult> GetAdminListAsync(
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
        CancellationToken cancellationToken = default);

    Task<Message> GetAdminDetailAsync(
        long id,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default);

    Task<MessageAdminHistoryResult> GetHistoryAdminAsync(
        Guid sessionUnitId,
        long? maxMessageId,
        int maxResultCount,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default);

    Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<ArticleContentInput> input,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default);

    Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, long>> RollbackAdminAsync(
        long messageId,
        MessageAdminScope scope = null,
        CancellationToken cancellationToken = default);
}
