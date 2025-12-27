using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.TextTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;


namespace IczpNet.Chat.SessionUnitSettings;

public class SessionUnitSettingManager(
    IDistributedCache<SessionUnitSettingCacheItem, Guid> sessionUnitSettingCache,
    IObjectMapper objectMapper,
    IMessageSender messageSender,
    ISessionUnitManager sessionUnitManager,
    ISessionUnitCacheManager sessionUnitCacheManager,
    ISessionUnitSettingRepository sessionUnitSettingRepository) : DomainService, ISessionUnitSettingManager
{
    public IDistributedCache<SessionUnitSettingCacheItem, Guid> SessionUnitSettingCache { get; } = sessionUnitSettingCache;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IMessageSender MessageSender { get; } = messageSender;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    public ISessionUnitSettingRepository SessionUnitSettingRepository { get; } = sessionUnitSettingRepository;

    protected Task<SessionUnitSetting> GetAsync(Guid sessionUnitId)
    {
        return SessionUnitSettingRepository.GetAsync(x => x.SessionUnitId == sessionUnitId);

    }

    public async IAsyncEnumerable<Guid> BatchSearchAsync(
        string keyword,
        int batchSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var totalCount = 0;
        var batchIndex = 0;

        Logger.LogInformation(
            "[BatchSearch] Start | Keyword={Keyword}, BatchSize={BatchSize}",
            keyword, batchSize);

        var baseQuery = (await SessionUnitSettingRepository.GetQueryableAsync())
            .Where(x =>
                x.Rename.Contains(keyword) ||
                x.RenameSpellingAbbreviation.Contains(keyword) ||
                x.RenameSpelling.Contains(keyword))
            .Select(x => new
            {
                x.SessionUnitId,
                x.CreationTime
            });

        DateTime? lastCreationTime = null;
        Guid? lastSessionUnitId = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batchStopwatch = Stopwatch.StartNew();

            var query = baseQuery;

            if (lastCreationTime.HasValue)
            {
                query = query.Where(x =>
                    x.CreationTime > lastCreationTime.Value ||
                    (x.CreationTime == lastCreationTime.Value &&
                     x.SessionUnitId.CompareTo(lastSessionUnitId!.Value) > 0));
            }

            var batch = await query
                .OrderBy(x => x.CreationTime)
                .ThenBy(x => x.SessionUnitId)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            batchStopwatch.Stop();

            if (batch.Count == 0)
            {
                Logger.LogInformation(
                    "[BatchSearch] Completed | Total={Total}, Batches={Batches}, Elapsed={Elapsed}ms",
                    totalCount,
                    batchIndex,
                    totalStopwatch.ElapsedMilliseconds);

                yield break;
            }

            batchIndex++;
            totalCount += batch.Count;

            var last = batch[^1];
            lastCreationTime = last.CreationTime;
            lastSessionUnitId = last.SessionUnitId;

            Logger.LogDebug(
                "[BatchSearch] Batch#{BatchIndex} | Count={Count}, LastTime={LastTime}, Elapsed={Elapsed}ms",
                batchIndex,
                batch.Count,
                lastCreationTime,
                batchStopwatch.ElapsedMilliseconds);

            foreach (var item in batch)
            {
                yield return item.SessionUnitId;
            }

        }
    }

    protected async Task<SessionUnitSetting> SetEntityAsync(Guid sessionUnitId, Action<SessionUnitSetting> action, bool autoSave = true)
    {
        var sessionUnitSetting = await GetAsync(sessionUnitId);

        action?.Invoke(sessionUnitSetting);

        // Update Cache
        var entity = await SessionUnitSettingRepository.UpdateAsync(sessionUnitSetting, autoSave);

        var sessionUnitSettingCacheItem = ObjectMapper.Map<SessionUnitSetting, SessionUnitSettingCacheItem>(entity);

        await SessionUnitSettingCache.SetAsync(sessionUnitId, sessionUnitSettingCacheItem);

        return entity;
    }

    public async Task<List<Guid>> SearchRenameIdsAsync(string keyword)
    {
        var searchIds = new List<Guid>();
        await foreach (var id in BatchSearchAsync(keyword, 1000))
        {
            searchIds.Add(id);
        }
        return searchIds;
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnitSetting> ClearMessageAsync(Guid sessionUnitId)
    {
        await SessionUnitManager.SetReadedMessageIdAsync(sessionUnitId, false);
        return await SetEntityAsync(sessionUnitId, x => x.ClearMessage(Clock.Now));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> DeleteMessageAsync(Guid sessionUnitId, long messageId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> KillAsync(Guid sessionUnitId)
    {
        return SetEntityAsync(sessionUnitId, x => x.Kill(Clock.Now));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> RemoveAsync(Guid sessionUnitId)
    {
        return SetEntityAsync(sessionUnitId, x => x.Remove(Clock.Now));
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnitSetting> SetImmersedAsync(Guid sessionUnitId, bool isImmersed)
    {
        var setting = await SetEntityAsync(sessionUnitId, x =>
        {
            Assert.If(x.Session?.IsEnableSetImmersed == false, "Session is disable to set immersed.");
            x.SetImmersed(isImmersed);
        });
        // change total stat for Immersed
        await SessionUnitCacheManager.ChangeImmersedAsync(sessionUnitId, isImmersed);
        return setting;
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> SetIsContactsAsync(Guid sessionUnitId, bool isContacts)
    {
        return SetEntityAsync(sessionUnitId, x => x.SetIsContacts(isContacts));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> SetIsShowMemberNameAsync(Guid sessionUnitId, bool isShowMemberName)
    {
        return SetEntityAsync(sessionUnitId, x => x.SetIsShowMemberName(isShowMemberName));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> SetMemberNameAsync(Guid sessionUnitId, string memberName)
    {
        return SetEntityAsync(sessionUnitId, x => x.SetMemberName(memberName));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitSetting> SetRenameAsync(Guid sessionUnitId, string rename)
    {
        return SetEntityAsync(sessionUnitId, x => x.SetRename(rename));
    }

    /// <inheritdoc />
    public virtual async Task<DateTime?> SetMuteExpireTimeAsync(Guid muterSessionUnitId, DateTime? muteExpireTime, SessionUnit setterSessionUnit, bool isSendMessage)
    {
        var setting = await GetAsync(muterSessionUnitId);

        var muterSessionUnit = setting.SessionUnit;

        Assert.If(setting.IsCreator, $"Creator can't be mute.");

        var allowList = new List<ChatObjectTypeEnums?> { ChatObjectTypeEnums.Room, ChatObjectTypeEnums.Square, ChatObjectTypeEnums.Official, ChatObjectTypeEnums.Subscription, };

        Assert.If(!allowList.Contains(muterSessionUnit.DestinationObjectType), $"DestinationObjectType '{muterSessionUnit.DestinationObjectType}' can't be mute.");

        await SetEntityAsync(muterSessionUnitId, x => x.SetMuteExpireTime(muteExpireTime));

        if (!isSendMessage)
        {
            return muteExpireTime;
        }

        if (setterSessionUnit == null)
        {
            Logger.LogWarning($"SetMuteExpireTime send message fial,SessionUnitId={muterSessionUnit.Id}");
            return muteExpireTime;
        }

        var timeSpan = muteExpireTime - Clock.Now;

        var isMuted = timeSpan.HasValue && timeSpan.Value.Milliseconds > 0;

        //sendMessage
        await MessageSender.SendCmdAsync(setterSessionUnit, new MessageInput<CmdContentInfo>()
        {
            Content = new CmdContentInfo()
            {
                Text = new TextTemplate(isMuted ? "'{muteObject}' 被禁言 {Minutes} 分钟" : "'{muteObject}' 被取消禁言")
                        .WithData("muteObject", new SessionUnitTextTemplate(muterSessionUnit))
                        .WithData("minutes", timeSpan?.Minutes)
                        .ToString(),
            }
        });

        return muteExpireTime;
    }

    public virtual async Task<List<SessionUnitSetting>> GetManyAsync(List<Guid> unitIdList)
    {
        return await SessionUnitSettingRepository.GetListAsync(x => unitIdList.Contains(x.SessionUnitId));
    }

    public virtual async Task<SessionUnitSettingCacheItem> GetOrAddCacheAsync(Guid unitId)
    {
        return await SessionUnitSettingCache.GetOrAddAsync(
            unitId,
            async () =>
            {
                var entity = await GetAsync(unitId);
                var result = ObjectMapper.Map<SessionUnitSetting, SessionUnitSettingCacheItem>(entity);
                return result;
            });
    }

    public virtual async Task<KeyValuePair<Guid, SessionUnitSettingCacheItem>[]> GetOrAddManyCacheAsync(List<Guid> unitIdList)
    {
        return await SessionUnitSettingCache.GetOrAddManyAsync(
            unitIdList,
            async (keys) =>
            {
                // 查询所有存在的数据
                var entities = await GetManyAsync(keys.ToList());

                // 生成字典方便查找
                var dict = entities.ToDictionary(
                    x => x.SessionUnitId,
                    x => ObjectMapper.Map<SessionUnitSetting, SessionUnitSettingCacheItem>(x)
                );

                // ⚠️ 必须给每一个 key 返回一个 KeyValuePair
                // ⚠️ 顺序必须与 keys 完全一致
                var result = new List<KeyValuePair<Guid, SessionUnitSettingCacheItem>>();

                foreach (var id in keys)
                {
                    dict.TryGetValue(id, out var cacheItem);
                    result.Add(new KeyValuePair<Guid, SessionUnitSettingCacheItem>(id, cacheItem));
                }

                return result;
            });
    }
}
