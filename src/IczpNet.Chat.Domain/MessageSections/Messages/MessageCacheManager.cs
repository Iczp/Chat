using IczpNet.Chat.RedisServices;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using IczpNet.Chat.SessionUnitSettings;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class MessageCacheManager : RedisService, IMessageCacheManager,
    ILocalEventHandler<EntityCreatedEventData<Message>>,
    ILocalEventHandler<EntityUpdatedEventData<Message>>,
    ILocalEventHandler<EntityDeletedEventData<Message>>,
    ILocalEventHandler<EntityCreatedEventData<DeletedRecorder>>,
    ILocalEventHandler<EntityDeletedEventData<DeletedRecorder>>,
    ILocalEventHandler<EntityUpdatedEventData<SessionUnitSetting>>,
    ILocalEventHandler<EntityDeletedEventData<SessionUnitSetting>>
{

    protected string MessagesPrefix => $"{Options.Value.KeyPrefix}Messages:";
    private MessageOptions Config => LazyServiceProvider.LazyGetRequiredService<IOptions<MessageOptions>>().Value;
    private RedisKey VersionKey(Guid id) => $"{MessagesPrefix}ReadWindow:{id}:Version";
    private RedisKey RecentKey(Guid id) => $"{MessagesPrefix}ReadWindow:{id}:Recent";
    private RedisKey HistoryKey(Guid id) => $"{MessagesPrefix}ReadWindow:{id}:History";
    private RedisKey DeletedKey(Guid id) => $"{MessagesPrefix}ReadDeleted:{id}";
    private RedisKey DeletedVersionKey(Guid id) => $"{MessagesPrefix}ReadDeleted:{id}:Version";
    private RedisKey SettingsKey(Guid id) => $"{MessagesPrefix}ReadSettings:{id}";
    private RedisKey SettingsVersionKey(Guid id) => $"{MessagesPrefix}ReadSettings:{id}:Version";
    private long ExpiryMilliseconds => Math.Max(1, (long)Config.ReadCacheExpiration.TotalMilliseconds);

    // A transactional query can see its own uncommitted changes. Never publish that snapshot to other readers.
    private bool CanPublishReadSnapshot =>
        LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>().Current?.Options?.IsTransactional != true;

    public async Task<(SessionMessageWindowCacheItem Recent, SessionMessageWindowCacheItem History, string Version)>
        GetReadWindowsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // MGET returns both windows and their generation from the same Redis snapshot (one RTT).
        var values = await Database.StringGetAsync([RecentKey(sessionId), HistoryKey(sessionId), VersionKey(sessionId)]);
        return (Deserialize(values[0]), Deserialize(values[1]), values[2].ToString());
    }

    private static SessionMessageWindowCacheItem Deserialize(RedisValue value)
        => value.IsNull ? null : JsonSerializer.Deserialize<SessionMessageWindowCacheItem>(value.ToString());

    public async Task<SessionUnitSettingCacheItem> GetReadSettingsAsync(Guid sessionUnitId,
        Func<Task<SessionUnitSettingCacheItem>> fetch, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var values = await Database.StringGetAsync([SettingsKey(sessionUnitId), SettingsVersionKey(sessionUnitId)]);
        if (!values[0].IsNull) return JsonSerializer.Deserialize<SessionUnitSettingCacheItem>(values[0].ToString());
        var settings = await fetch();
        if (!CanPublishReadSnapshot) return settings;
        await Database.ScriptEvaluateAsync(@"
if (redis.call('GET', KEYS[1]) or '') ~= ARGV[1] then return 0 end
redis.call('SET', KEYS[2], ARGV[2], 'PX', ARGV[3])
return 1", [SettingsVersionKey(sessionUnitId), SettingsKey(sessionUnitId)],
            [values[1].ToString(), JsonSerializer.Serialize(settings), ExpiryMilliseconds]);
        return settings;
    }

    public async Task<bool> StoreReadWindowAsync(Guid sessionId, SessionMessageWindowCacheItem window, string version,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CanPublishReadSnapshot) return false;
        var key = window.MaxMessageId == long.MaxValue ? RecentKey(sessionId) : HistoryKey(sessionId);
        var result = await Database.ScriptEvaluateAsync(@"
if (redis.call('GET', KEYS[1]) or '') ~= ARGV[1] then return 0 end
redis.call('SET', KEYS[2], ARGV[2], 'PX', ARGV[3])
return 1", [VersionKey(sessionId), key], [version ?? "", JsonSerializer.Serialize(window), ExpiryMilliseconds]);
        return (long)result == 1;
    }

    public async Task<HashSet<long>> GetDeletedIdsAsync(Guid sessionUnitId, List<long> ids,
        Func<List<long>, Task<List<long>>> fetch, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ids.Count == 0) return [];
        var snapshot = (RedisResult[])await Database.ScriptEvaluateAsync(@"
local result = {redis.call('GET', KEYS[1]) or ''}
local flags = redis.call('HMGET', KEYS[2], unpack(ARGV))
for i=1,#flags do result[#result+1] = flags[i] end
return result", [DeletedVersionKey(sessionUnitId), DeletedKey(sessionUnitId)], ids.Select(x => (RedisValue)x).ToArray());
        var deleted = new HashSet<long>();
        var missing = new List<long>();
        for (var i = 0; i < ids.Count; i++)
        {
            if (snapshot[i + 1].IsNull) missing.Add(ids[i]);
            else if (snapshot[i + 1].ToString() == "1") deleted.Add(ids[i]);
        }
        if (missing.Count == 0) return deleted;
        var fetched = (await fetch(missing)).ToHashSet();
        deleted.UnionWith(fetched);
        if (!CanPublishReadSnapshot) return deleted;
        var arguments = new List<RedisValue> { snapshot[0].ToString(), ExpiryMilliseconds };
        foreach (var id in missing) { arguments.Add(id); arguments.Add(fetched.Contains(id) ? "1" : "0"); }
        await Database.ScriptEvaluateAsync(@"
if (redis.call('GET', KEYS[1]) or '') ~= ARGV[1] then return 0 end
-- Bound the per-member negative cache; eviction only causes bounded database fallback.
local ttl = redis.call('PTTL', KEYS[2])
if redis.call('HLEN', KEYS[2]) + (#ARGV-2)/2 > 4096 then
    redis.call('DEL', KEYS[2])
    ttl = -2
end
for i=3,#ARGV,2 do redis.call('HSET', KEYS[2], ARGV[i], ARGV[i+1]) end
-- Preserve the original expiration, so a missed invalidation cannot live forever under continuous writes.
redis.call('PEXPIRE', KEYS[2], ttl >= 0 and math.max(1,ttl) or ARGV[2])
return 1", [DeletedVersionKey(sessionUnitId), DeletedKey(sessionUnitId)], arguments.ToArray());
        return deleted;
    }

    private void AfterCommit(Func<MessageCacheManager, Task> action)
    {
        var uow = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>().Current;
        if (uow == null) throw new InvalidOperationException("Message read cache mutation requires a unit of work.");
        var scopeFactory = LazyServiceProvider.LazyGetRequiredService<IServiceScopeFactory>();
        uow.OnCompleted(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var manager = (MessageCacheManager)scope.ServiceProvider.GetRequiredService<IMessageCacheManager>();
            await action(manager);
        });
    }

    public Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        // Local entity events run before the transaction commits; update certified windows only after commit.
        AfterCommit(manager => manager.AppendCommittedAsync(eventData.Entity));
        return Task.CompletedTask;
    }

    private async Task AppendCommittedAsync(Message message)
    {
        if (!message.SessionId.HasValue || message.IsDeleted) return;
        var item = new SessionMessageWindowCacheItem.Entry
        {
            Id = message.Id, IsPrivate = message.IsPrivate, SenderSessionUnitId = message.SenderSessionUnitId,
            ReceiverSessionUnitId = message.ReceiverSessionUnitId, CreationTime = message.CreationTime
        };
        await Database.ScriptEvaluateAsync(@"
-- Ids are JSON strings: compare without Lua double precision loss.
local function less(a,b) if #a ~= #b then return #a < #b end return a < b end
redis.call('SET', KEYS[1], ARGV[1])
local entry = cjson.decode(ARGV[2])
local recent = redis.call('GET', KEYS[2])
if recent then
    local window = cjson.decode(recent)
    if less(window.MinMessageId, entry.Id) and less(entry.Id, window.MaxMessageId) then
        local items = {}
        local inserted = false
        for _,old in ipairs(window.Items) do
            if old.Id == entry.Id then
                items[#items+1] = entry
                inserted = true
            else
                if not inserted and less(entry.Id,old.Id) then
                    items[#items+1] = entry
                    inserted = true
                end
                items[#items+1] = old
            end
        end
        if not inserted then items[#items+1] = entry end
        while #items > tonumber(ARGV[3]) do window.MinMessageId = table.remove(items,1).Id end
        window.Items = items
        -- Keep the certification's original expiry instead of extending it on every append.
        local ttl = redis.call('PTTL', KEYS[2])
        redis.call('SET', KEYS[2], cjson.encode(window), 'PX', ttl >= 0 and math.max(1,ttl) or ARGV[4])
    end
end
local history = redis.call('GET', KEYS[3])
if history then
    local window = cjson.decode(history)
    -- Normal new messages preserve older windows; late commits inside a window invalidate it.
    if less(window.MinMessageId, entry.Id) and less(entry.Id, window.MaxMessageId) then
        redis.call('DEL', KEYS[3])
    end
end
return 1", [VersionKey(message.SessionId.Value), RecentKey(message.SessionId.Value), HistoryKey(message.SessionId.Value)],
            [Guid.NewGuid().ToString("N"), JsonSerializer.Serialize(item), Math.Clamp(Config.ReadCacheBatchSize, 16, 512), ExpiryMilliseconds]);
    }

    private Task InvalidateWindowsAsync(Guid sessionId) => Database.ScriptEvaluateAsync(@"
redis.call('SET', KEYS[1], ARGV[1])
redis.call('DEL', KEYS[2], KEYS[3])
return 1", [VersionKey(sessionId), RecentKey(sessionId), HistoryKey(sessionId)], [Guid.NewGuid().ToString("N")]);

    private async Task MessageChangedAsync(Message message)
    {
        if (!message.SessionId.HasValue) return;
        await InvalidateWindowsAsync(message.SessionId.Value);
        AfterCommit(manager => manager.InvalidateWindowsAsync(message.SessionId.Value));
    }

    public Task HandleEventAsync(EntityUpdatedEventData<Message> eventData) => MessageChangedAsync(eventData.Entity);
    public Task HandleEventAsync(EntityDeletedEventData<Message> eventData) => MessageChangedAsync(eventData.Entity);

    private async Task DeletedChangedAsync(DeletedRecorder recorder)
    {
        await InvalidateDeletedAsync(recorder.SessionUnitId);
        AfterCommit(manager => manager.InvalidateDeletedAsync(recorder.SessionUnitId));
    }

    private async Task InvalidateDeletedAsync(Guid sessionUnitId)
    {
        await Database.ScriptEvaluateAsync(@"
redis.call('SET', KEYS[1], ARGV[1])
redis.call('DEL', KEYS[2])
return 1", [DeletedVersionKey(sessionUnitId), DeletedKey(sessionUnitId)], [Guid.NewGuid().ToString("N")]);
        await LazyServiceProvider.LazyGetRequiredService<IDeletedRecorderManager>().RemoveCacheAsync(sessionUnitId);
    }

    public Task HandleEventAsync(EntityCreatedEventData<DeletedRecorder> eventData) => DeletedChangedAsync(eventData.Entity);
    public Task HandleEventAsync(EntityDeletedEventData<DeletedRecorder> eventData) => DeletedChangedAsync(eventData.Entity);

    private Task InvalidateSettingsAsync(Guid sessionUnitId) => Database.ScriptEvaluateAsync(@"
redis.call('SET', KEYS[1], ARGV[1])
redis.call('DEL', KEYS[2])
return 1", [SettingsVersionKey(sessionUnitId), SettingsKey(sessionUnitId)], [Guid.NewGuid().ToString("N")]);

    private async Task SettingsChangedAsync(SessionUnitSetting settings)
    {
        await InvalidateSettingsAsync(settings.SessionUnitId);
        AfterCommit(manager => manager.InvalidateSettingsAsync(settings.SessionUnitId));
    }

    public Task HandleEventAsync(EntityUpdatedEventData<SessionUnitSetting> eventData) => SettingsChangedAsync(eventData.Entity);
    public Task HandleEventAsync(EntityDeletedEventData<SessionUnitSetting> eventData) => SettingsChangedAsync(eventData.Entity);
}
