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

    Task<PagedResultDto<SessionUnitFriendDto>> GetChangesAsync(SessionUnitChangesGetListInput input);

    Task<PagedResultDto<SessionUnitFriendDto>> GetFriendsAsync(SessionUnitFirendGetListInput input);

    Task<FriendCountDto> GetFriendsCountAsync([Required] long ownerId);

    Task<PagedResultDto<SessionUnitMemberDto>> GetMembersAsync(SessionUnitMemberGetListInput input);

    Task<MemberCountDto> GetMembersCountAsync(Guid sessionId);

    Task<SessionUnitFriendDto> GetAsync(Guid id);

    Task<SessionUnitFriendDetailDto> GetDetailAsync(Guid id);

    Task<SessionUnitOverviewInfo> GetOverviewOwnerAsync(long ownerId);

    Task<List<SessionUnitOverviewInfo>> GetOverviewOwnersAsync(List<long> ownerIds);

    Task<List<SessionUnitOverviewInfo>> GetOverviewUserAsync([Required] Guid userId);

    Task<List<SessionUnitOverviewInfo>> GetOverviewAsync();

    Task<Dictionary<long, IEnumerable<BoxBadgeInfo>>> GetBoxOverviewAsync(List<long> ownerIds);

}
