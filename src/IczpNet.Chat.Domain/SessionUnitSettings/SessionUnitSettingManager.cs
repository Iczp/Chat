
using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.TextTemplates;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
    ISessionUnitSettingRepository sessionUnitSettingRepository) : DomainService, ISessionUnitSettingManager
{
    public IDistributedCache<SessionUnitSettingCacheItem, Guid> SessionUnitSettingCache { get; } = sessionUnitSettingCache;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IMessageSender MessageSender { get; } = messageSender;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitSettingRepository SessionUnitSettingRepository { get; } = sessionUnitSettingRepository;

    protected Task<SessionUnitSetting> GetAsync(Guid sessionUnitId)
    {
        return SessionUnitSettingRepository.GetAsync(x => x.SessionUnitId == sessionUnitId);

    }
    protected async Task<SessionUnitSetting> SetEntityAsync(Guid sessionUnitId, Action<SessionUnitSetting> action, bool autoSave = true)
    {
        var sessionUnitSetting = await GetAsync(sessionUnitId);
        action?.Invoke(sessionUnitSetting);
        await SessionUnitSettingCache.RemoveAsync(sessionUnitId);
        return await SessionUnitSettingRepository.UpdateAsync(sessionUnitSetting, autoSave);
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
    public virtual Task<SessionUnitSetting> SetImmersedAsync(Guid sessionUnitId, bool isImmersed)
    {
        return SetEntityAsync(sessionUnitId, x =>
        {
            Assert.If(!x.Session.IsEnableSetImmersed, "Session is disable to set immersed.");
            x.SetImmersed(isImmersed);
        });
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

    public virtual async Task<IDictionary<Guid, SessionUnitSettingCacheItem>> GetManyByCacheAsync(List<Guid> unitIdList)
    {
        var list = await SessionUnitSettingCache.GetOrAddManyAsync(unitIdList, async (unitIdList) =>
        {
            var list = (await GetManyAsync(unitIdList.ToList()));

            var items = ObjectMapper.Map<List<SessionUnitSetting>, List<SessionUnitSettingCacheItem>>(list);

            return items.Select(x => new KeyValuePair<Guid, SessionUnitSettingCacheItem>(x.SessionUnitId, x)).ToList();

        });
        return list.ToDictionary(x => x.Key, x => x.Value);
    }
}
