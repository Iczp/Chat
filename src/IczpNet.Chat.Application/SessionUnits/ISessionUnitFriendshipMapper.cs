using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitFriendshipMapper : IObjectMapper<SessionUnitCacheItem, SessionUnitFriendshipDto>, ITransientDependency
{
}
