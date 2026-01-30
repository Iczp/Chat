using IczpNet.AbpCommons;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectResolver : DomainService, IChatObjectResolver
{
    protected IDistributedCache<List<long>, Guid> SessionUnitIdListCache { get; }

    protected IDistributedCache<List<SessionUnitCacheItem>, Guid> SessionUnitCache { get; }

    public ChatObjectResolver(
        IDistributedCache<List<long>, Guid> sessionUnitIdListCache)
    {
        SessionUnitIdListCache = sessionUnitIdListCache;
    }

    public virtual async Task<List<ChatObject>> GetListAsync(Message message)
    {
        await Task.Yield();

        var result = new List<ChatObject>();

        switch (message.Channel)
        {
            case Enums.Channels.PrivateChannel:
                return new List<ChatObject>() { message.Sender, message.Receiver };
            case Enums.Channels.RoomChannel:
                break;
            case Enums.Channels.SubscriptionChannel:
                break;
            case Enums.Channels.ServiceChannel:
                break;
            case Enums.Channels.SquareChannel:
                break;
            case Enums.Channels.RobotChannel:
                break;
            case Enums.Channels.ElectronicCommerceChannel:
                break;
            default:
                break;
        }
        return result;
    }

    public virtual Task<List<long>> GetIdListAsync(Message message)
    {
        Assert.NotNull(message.SessionId, "Message.Session is not null.");

        return SessionUnitIdListCache.GetOrAddAsync(message.SessionId.Value, () =>
        {
            return Task.FromResult(message.Session.UnitList.Select(x => x.OwnerId).ToList());
        });
    }
}
