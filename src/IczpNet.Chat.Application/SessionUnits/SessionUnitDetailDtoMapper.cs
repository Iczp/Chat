using IczpNet.Chat.SessionUnits.Dtos;
using System;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitMemberDetailDtoMapper : DomainService, IObjectMapper<SessionUnitCacheItem, SessionUnitMemberDetailDto>
{
    public SessionUnitMemberDetailDto Map(SessionUnitCacheItem source)
    {


        return new SessionUnitMemberDetailDto()
        {
            Id = source.Id,
            SessionId = source.SessionId,
            MemberName = source.MemberName,

            OwnerId = source.OwnerId,
            OwnerObjectType = source.OwnerObjectType,

            DestinationId = source.DestinationId,
            DestinationObjectType = source.DestinationObjectType,

            Sorting = source.Sorting,
            Ticks = source.Ticks,

        };
    }

    public SessionUnitMemberDetailDto Map(SessionUnitCacheItem source, SessionUnitMemberDetailDto destination)
    {
        if (destination.Setting != null)
        {
            destination.Setting.LastSendMessageId = source.LastSendMessageId;
            destination.Setting.LastSendTime = source.LastSendTime;

            destination.Setting.SessionUnitId = Guid.Empty;
        }

        return destination;

    }
}
