using IczpNet.Chat.SessionUnitSettings.Dtos;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionUnitSettings;

public interface ISessionUnitMemberSettingMapper : IObjectMapper<SessionUnitSettingCacheItem, SessionUnitMemberSettingDto>
{
}
