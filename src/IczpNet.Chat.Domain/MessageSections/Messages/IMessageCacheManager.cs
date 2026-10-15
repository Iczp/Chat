namespace IczpNet.Chat.MessageSections.Messages;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.SessionUnitSettings;

public interface IMessageCacheManager
{
    Task<List<MessageChangeEntry>> GetChangeWindowAsync(Guid sessionId, Guid epoch, long after, long through,
        int take, Func<Task<List<MessageChangeEntry>>> fetch, CancellationToken cancellationToken = default);
    Task<SessionUnitSettingCacheItem> GetReadSettingsAsync(Guid sessionUnitId,
        Func<Task<SessionUnitSettingCacheItem>> fetch, CancellationToken cancellationToken = default);
    Task<(SessionMessageWindowCacheItem Recent, SessionMessageWindowCacheItem History, string Version)>
        GetReadWindowsAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> StoreReadWindowAsync(Guid sessionId, SessionMessageWindowCacheItem window, string version,
        CancellationToken cancellationToken = default);
    Task<HashSet<long>> GetDeletedIdsAsync(Guid sessionUnitId, List<long> ids,
        Func<List<long>, Task<List<long>>> fetch, CancellationToken cancellationToken = default);
}
