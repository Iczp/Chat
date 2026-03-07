using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitFriendshipMapper : DomainService, ISessionUnitFriendshipMapper
{
    public SessionUnitFriendshipDto Map(SessionUnitCacheItem source)
    {
        return new SessionUnitFriendshipDto()
        {
            IsFriendship = true,
            DisplayName = source.Rename,
            SessionUnitId = source.Id,
        };
    }

    public SessionUnitFriendshipDto Map(SessionUnitCacheItem source, SessionUnitFriendshipDto destination)
    {
        destination.IsFriendship = true;
        destination.DisplayName = source.Rename;
        destination.SessionUnitId = source.Id;
        return destination;
    }
}
