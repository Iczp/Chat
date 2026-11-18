using IczpNet.Chat.SessionUnitSettings.Dtos;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnitSettings;

public class SessionUnitMemberSettingMapper //: DomainService, ISessionUnitMemberSettingMapper
{
    public SessionUnitMemberSettingDto Map(SessionUnitSettingCacheItem source)
    {
        return new SessionUnitMemberSettingDto();
    }

    public SessionUnitMemberSettingDto Map(SessionUnitSettingCacheItem source, SessionUnitMemberSettingDto destination)
    {
        throw new System.NotImplementedException();
    }
}
