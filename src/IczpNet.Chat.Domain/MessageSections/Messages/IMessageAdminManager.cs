using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Templates;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageAdminHistoryResult
{
    public long TotalCount { get; set; }

    public List<MessageCacheItem> Items { get; set; } = new();

    public bool HasMore { get; set; }

    public long? NextCursorId { get; set; }
}

public interface IMessageAdminManager
{
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
