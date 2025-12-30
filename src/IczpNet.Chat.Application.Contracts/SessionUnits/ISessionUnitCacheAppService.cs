using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitCacheAppService
{
    Task<PagedResultDto<SessionUnitFriendDto>> GetListAsync(SessionUnitCacheItemGetListInput input);

    Task<List<SessionUnitFriendDto>> GetManyAsync(List<Guid> unitIds);

    Task<PagedResultDto<SessionUnitFriendDto>> GetFriendsAsync([Required] long ownerId, SessionUnitCacheScoreGetListInput input);

    Task<PagedResultDto<SessionUnitFriendDto>> GetLatestAsync([Required] long ownerId, SessionUnitCacheScoreGetListInput input);

    Task<SessionUnitFriendDto> GetAsync(Guid id);

    Task<BadgeDto> GetBadgeAsync(long ownerId);

    Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null);

    Task<long> GetMembersCountAsync(Guid sessionId);


}
