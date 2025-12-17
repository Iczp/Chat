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

    Task<PagedResultDto<SessionUnitCacheDto>> GetHistoryAsync(SessionUnitCacheScoreGetListInput input);

    Task<PagedResultDto<SessionUnitCacheDto>> GetLatestAsync(SessionUnitCacheScoreGetListInput input);

    Task<SessionUnitCacheDto> GetAsync(Guid id);

    Task<SessionUnitStatistic> GetStatisticAsync(long ownerId);

    Task<List<BadgeDto>> GetStatisticByUserIdAsync([Required] Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetStatisticByCurrentUserAsync(bool? isImmersed = null);
}
