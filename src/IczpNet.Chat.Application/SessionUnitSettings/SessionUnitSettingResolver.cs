using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings.Dtos;

namespace IczpNet.Chat.SessionUnitSettings;

public class SessionUnitSettingResolver : MapperResolver, IValueResolver<SessionUnit, SessionUnitOwnerDto, SessionUnitSettingDto>
{
    public SessionUnitSettingDto Resolve(SessionUnit source, SessionUnitOwnerDto destination, SessionUnitSettingDto destMember, ResolutionContext context)
    {
        if (source.Setting == null)
        {
            return null;
        }
        return ObjectMapper.Map<SessionUnitSetting, SessionUnitSettingDto>(source.Setting);
    }
}