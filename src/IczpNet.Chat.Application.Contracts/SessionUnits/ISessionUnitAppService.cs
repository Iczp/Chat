using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitAppService
{
    Task<Guid> FindIdAsync([Required] long ownerId, [Required] long destinactionId);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitDto>> GetListFastAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitDestinationDto>> GetListDestinationAsync(Guid id, SessionUnitGetListDestinationInput input);
    Task<PagedResultDto<SessionUnitDestinationDto>> GetMembersAsync(Guid id, SessionUnitGetListDestinationInput input);

    Task<List<long>> GetListDestinationOwnerIdListAsync(Guid id);

    Task<PagedResultDto<SessionUnitDto>> GetListSameDestinationAsync(SessionUnitGetListSameDestinationInput input);

    Task<PagedResultDto<SessionUnitDto>> GetListSameSessionAsync(SessionUnitGetListSameSessionInput input);

    Task<int> GetSameSessionCountAsync([Required] long sourceId, [Required] long targetId, List<ChatObjectTypeEnums> objectTypeList);

    Task<int> GetSameDestinationCountAsync([Required] long sourceId, [Required] long targetId, List<ChatObjectTypeEnums> objectTypeList);

    Task<SessionUnitOwnerDto> GetAsync(Guid id);

    Task<SessionUnitCacheItem> GetByCacheAsync(Guid id);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetManyAsync(List<Guid> idList);

    Task<SessionUnitDetailDto> GetDetailAsync(Guid id);

    Task<SessionUnitDestinationDto> GetDestinationAsync(Guid id, Guid destinationId);

    Task<BadgeDto> GetBadgeByIdAsync(Guid id, bool? isImmersed = null);

    Task<Dictionary<ChatObjectTypeEnums, int>> GetTypedBadgeByOwnerIdAsync([Required] long ownerId, bool? isImmersed = null);

    Task<BadgeDto> GetBadgeByOwnerIdAsync([Required] long ownerId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null);

    Task<PagedResultDto<SessionUnitCacheItem>> GetListCachesAsync(SessionUnitCacheGetListInput input);

    Task<SessionUnitCacheItem> GetCacheAsync([Required] Guid sessionUnitId);

    Task<SessionUnitCounterInfo> GetCounterAsync(SessionUnitGetCounterInput input);

    /// <summary>
    /// 根据创建用户创建相应的会话(通知\新闻\机器人等)
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    Task<int> GenerateDefaultSessionAsync(long chatObjectId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="ticks"></param>
    /// <returns></returns>
    Task<long> UpdateTicksAsync(Guid sessionUnitId, long? ticks);

    /// <summary>
    /// 清除角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<long> ClearBadgeAsync(long ownerId);

    /// <summary>
    /// 设置会话盒子
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="boxId"></param>
    /// <returns></returns>
    Task<SessionUnitCacheItem> SetBoxAsync([Required] Guid sessionUnitId, [Required] Guid boxId);
}
