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
        CancellationToken cancellationToken = default);

    Task<Message> GetAdminDetailAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<MessageAdminHistoryResult> GetHistoryAdminAsync(
        Guid sessionUnitId,
        long? maxMessageId,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<ArticleContentInput> input,
        CancellationToken cancellationToken = default);

    Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAdminAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, long>> RollbackAdminAsync(
        long messageId,
        CancellationToken cancellationToken = default);
}
