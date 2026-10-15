using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class MessageCacheManager
{
    public async Task<List<MessageChangeEntry>> GetChangeWindowAsync(Guid sessionId, Guid epoch, long after, long through,
        int take, Func<Task<List<MessageChangeEntry>>> fetch, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (after >= through) return [];
        // The database supplies the committed upper boundary. Append-only rows
        // in this exact range cannot change; empty pages are safe to cache too.
        var key = (RedisKey)$"{MessagesPrefix}Changes:v1:{sessionId}:{epoch}:{after}:{through}:{take}";
        try
        {
            var cached = await Database.StringGetAsync(key);
            if (!cached.IsNull)
                return JsonSerializer.Deserialize<List<MessageChangeEntry>>(cached.ToString()) ?? [];
        }
        catch (Exception ex) when (ex is RedisException or JsonException) { }
        var entries = await fetch();
        try { await Database.StringSetAsync(key, JsonSerializer.Serialize(entries), Config.ReadCacheExpiration); }
        catch (RedisException) { /* SQL remains authoritative; a cache failure cannot lose a change. */ }
        return entries;
    }
}
