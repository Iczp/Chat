using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitFriendshipMapper : DomainService, ISessionUnitFriendshipMapper
{
    public SessionUnitFriendshipDto Map(SessionUnitCacheItem source)
    {
        return new SessionUnitFriendshipDto()
        {
            SessionUnitId = source.Id,
            ObjectType = source.OwnerObjectType,
            IsFriendship = true,
            DisplayName = source.Rename,
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
