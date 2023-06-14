using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public interface ISessionUnitAppService
{
    Task<Guid> FindIdAsync(long ownerId, long destinactionId);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitDestinationDto>> GetListDestinationAsync(Guid id, SessionUnitGetListDestinationInput input);

    Task<PagedResultDto<SessionUnitDto>> GetListSameDestinationAsync(SessionUnitGetListSameDestinationInput input);

    Task<PagedResultDto<SessionUnitDto>> GetListSameSessionAsync(SessionUnitGetListSameSessionInput input);

    Task<int> GetSameSessionCountAsync(long sourceId, long targetId, List<ChatObjectTypeEnums> objectTypeList);

    Task<int> GetSameDestinationCountAsync(long sourceId, long targetId, List<ChatObjectTypeEnums> objectTypeList);

    Task<SessionUnitOwnerDto> GetAsync(Guid id);

    Task<SessionUnitOwnerDetailDto> GetDetailAsync(Guid id);

    Task<SessionUnitDestinationDto> GetDestinationAsync(Guid id, Guid destinationId);

    Task<BadgeDto> GetBadgeByIdAsync(Guid id, bool? isImmersed = null);

    Task<BadgeDto> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByUserIdAsync(Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null);

    Task<PagedResultDto<SessionUnitCacheItem>> GetListCachesAsync(SessionUnitCacheGetListInput input);

    Task<SessionUnitCacheItem> GetCacheAsync(Guid sessionUnitId);

    Task<SessionUnitCounterInfo> GetCounterAsync(Guid sessionUnitId, long minMessageId = 0, bool? isImmersed = null);
}
