using IczpNet.Chat.SessionUnits.Dtos;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionUnits
{
    public class SessionUnitOwnerDtoMapper //: DomainService, IObjectMapper<SessionUnit, SessionUnitOwnerDto>, ITransientDependency
    {
        public SessionUnitOwnerDto Map(SessionUnit source)
        {
            return new SessionUnitOwnerDto()
            {
                Id = source.Id,
                Ticks = 456,
            };
        }

        public SessionUnitOwnerDto Map(SessionUnit source, SessionUnitOwnerDto destination)
        {
            destination.Ticks = 123;

            return destination;
        }
    }
}
