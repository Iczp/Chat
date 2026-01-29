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

    Task<PagedResultDto<SessionUnitFriendDto>> GetLatestAsync(SessionUnitLatestGetListInput input);

    Task<PagedResultDto<SessionUnitFriendDto>> GetFriendsAsync(SessionUnitFirendGetListInput input);

    Task<FriendCountDto> GetFriendsCountAsync([Required] long ownerId);

    Task<PagedResultDto<SessionUnitMemberDto>> GetMembersAsync(SessionUnitMemberGetListInput input);

    Task<MemberCountDto> GetMembersCountAsync(Guid sessionId);

    Task<SessionUnitFriendDto> GetAsync(Guid id);

    Task<SessionUnitFriendDetailDto> GetDetailAsync(Guid id);

    Task<BadgeDto> GetBadgeAsync(long ownerId);

    Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync();

}
