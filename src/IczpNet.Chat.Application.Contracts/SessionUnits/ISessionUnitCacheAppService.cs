using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
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

    Task<ExtraPagedResultDto<SessionUnitFriendDto>> GetFriendsAsync(SessionUnitFirendGetListInput input);

    Task<ExtraPagedResultDto<Guid>> GetFriendIdsAsync(SessionUnitFirendGetListInput input);

    Task<ExtraPagedResultDto<SessionUnitIndexDto>> GetFriendsIndexedAsync(long ownerId, ChatObjectTypeEnums? type);

    Task<FriendCountDto> GetFriendsCountAsync([Required] long ownerId);

    Task<ExtraPagedResultDto<SessionUnitMemberDto>> GetMembersAsync(SessionUnitMemberGetListInput input);

    Task<MemberCountDto> GetMembersCountAsync(Guid sessionId);

    Task<SessionUnitFriendDto> GetAsync(Guid id);

    Task<SessionUnitFriendDetailDto> GetFriendAsync(Guid unitId);

    Task<SessionUnitMemberDetailDto> GetMemberAsync(Guid unitId, SessionUnitGetMemberOptions options);

    Task<SessionUnitOwnerOverviewInfo> GetOverviewOwnerAsync(long ownerId);

    Task<List<SessionUnitOwnerOverviewInfo>> GetOverviewOwnersAsync(List<long> ownerIds);

    Task<SessionUnitUserOverviewInfo> GetOverviewUserAsync([Required] Guid userId);

    Task<SessionUnitUserOverviewInfo> GetOverviewAsync();

    Task<Dictionary<long, IEnumerable<SessionUnitStatInfo>>> GetBoxOverviewAsync(List<long> ownerIds);

    Task<long> GetDirtyCountAsync();

}
