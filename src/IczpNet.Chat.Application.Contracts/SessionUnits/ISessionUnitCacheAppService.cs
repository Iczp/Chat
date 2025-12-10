using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitCacheAppService
{
    Task<PagedResultDto<SessionUnitCacheDto>> GetListAsync(SessionUnitCacheItemGetListInput input);

    Task<List<SessionUnitCacheDto>> GetManyAsync(List<Guid> unitIds);

    Task<PagedResultDto<SessionUnitCacheDto>> GetLatestAsync(long ownerId, double minScore, long? maxResultCount = 20, long? skipCount = 0);

    Task<SessionUnitCacheDto> GetAsync(Guid id);



    Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null);

    Task<SessionUnitCacheItem> UnitTestAsync();
}
