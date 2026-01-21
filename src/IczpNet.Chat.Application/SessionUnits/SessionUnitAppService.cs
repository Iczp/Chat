using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Dtos;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Uow;
using Volo.Abp.Users;


namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话单元
/// </summary>
public class SessionUnitAppService(
    IMessageRepository messageRepository,
    ISessionUnitRepository repository,
    ISessionUnitCacheManager sessionUnitCacheManager,
    IReadedRecorderManager readedRecorderManager,
    IOpenedRecorderManager openedRecorderManager,
    IFavoritedRecorderManager favoriteManager,
    ISessionUnitSettingRepository sessionUnitSettingRepository,
    IFollowManager followManager) : ChatAppService, ISessionUnitAppService
{
    protected override string GetListPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected override string GetPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected virtual string GetDetailPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected virtual string GetListForSameSessionPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetSameSession;
    protected virtual string GetItemForSameSessionPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetSameSession;
    protected virtual string GetBadgePolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetBadge;
    protected virtual string FindPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.Find;
    protected virtual string GetCounterPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetCounter;

    protected ISessionUnitRepository Repository { get; } = repository;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IReadedRecorderManager ReadedRecorderManager { get; } = readedRecorderManager;
    protected IOpenedRecorderManager OpenedRecorderManager { get; } = openedRecorderManager;
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; } = favoriteManager;
    public ISessionUnitSettingRepository SessionUnitSettingRepository { get; } = sessionUnitSettingRepository;
    protected IFollowManager FollowManager { get; } = followManager;

    /// <inheritdoc/>
    protected override Task CheckPolicyAsync(string policyName)
    {
        return base.CheckPolicyAsync(policyName);
    }


    /// <inheritdoc/>
    protected virtual async Task<SessionUnit> GetEntityAsync(Guid id, bool checkIsKilled = true)
    {
        var entity = await Repository.GetAsync(id);

        var setting = await SessionUnitSettingRepository.GetAsync(x => x.SessionUnitId == id);

        Assert.If(checkIsKilled && (setting?.IsKilled ?? false), "已经删除的会话单元!");

        return entity;
    }

    /// <inheritdoc/>
    protected virtual async Task<IQueryable<SessionUnit>> CreateQueryAsync(SessionUnitGetListInput input)
    {
        //return from a in (await Repository.GetQueryableAsync())
        //       join b in await SessionUnitCounterRepository.GetQueryableAsync() on a.Id equals b.SessionUnitId
        //       select a;

        return (await Repository.GetQueryableAsync())
            //.Include(x => x.Setting)
            //.Where(x => x.Counter != null)
            //.Select(x => x.Counter.SessionUnit)
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.DestinationObjectType == input.DestinationObjectType)
            .WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            .WhereIf(input.IsCreator.HasValue, x => x.Setting.IsCreator == input.IsCreator)
            .WhereIf(input.MinMessageId.HasValue, x => x.LastMessageId >= input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.LastMessageId < input.MaxMessageId)
            .WhereIf(input.MinTicks.HasValue, x => x.Ticks >= input.MinTicks)
            .WhereIf(input.MaxTicks.HasValue, x => x.Ticks < input.MaxTicks)
            .WhereIf(input.IsTopping == true, x => x.Sorting > 0)
            .WhereIf(input.IsTopping == false, x => x.Sorting == 0)
            .WhereIf(input.IsContacts.HasValue, x => x.Setting.IsContacts == input.IsContacts)
            .WhereIf(input.IsImmersed.HasValue, x => x.Setting.IsImmersed == input.IsImmersed)
            .WhereIf(input.IsBadge.HasValue, x => x.PublicBadge > 0)

            //@我
            .WhereIf(input.IsRemind == true, x => (x.RemindAllCount + x.RemindMeCount) > 0)
            .WhereIf(input.IsRemind == false, x => (x.RemindAllCount + x.RemindMeCount) == 0)


            //我关注的
            //.WhereIf(input.IsFollowing.HasValue, x => x.FollowingCount > 0)
            .WhereIf(input.IsFollowing == true, x => x.FollowingList.Count > 0)
            .WhereIf(input.IsFollowing == false, x => x.FollowingList.Count == 0)

            //关注我的
            .WhereIf(input.IsFollower == true, x => x.FollowerList.Count > 0)
            .WhereIf(input.IsFollower == false, x => x.FollowerList.Count == 0)

            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;
    }

    /// <summary>
    /// 会话单元列表（消息总线）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadCommitted)]
    public virtual async Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName));

        var query = await CreateQueryAsync(input);

        return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
            query,
            input,
            x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
    }
    /// <summary>
    /// 会话单元列表（消息总线） - Fast
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadCommitted)]
    public virtual async Task<PagedResultDto<SessionUnitDto>> GetListFastAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName));

        var query = await CreateQueryAsync(input);

        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(
            query,
            input,
            x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
    }

    /// <summary>
    /// 消息统计
    /// </summary>
    /// <param name="idList">会话单元Id</param>
    /// <param name="minMessageId">最小消息Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> idList, long minMessageId)
    {
        return SessionUnitManager.GetStatsAsync(idList, minMessageId);
    }

    /// <summary>
    /// 获取目标列表（好友、群、群成员、机器人等）
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [Obsolete($"替代方法: {nameof(GetMembersAsync)}")]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListDestinationAsync(Guid id, SessionUnitGetListDestinationInput input)
    {
        return await GetMembersAsync(id, input);
    }
    /// <summary>
    /// 获取成员列表（好友、群、群成员、机器人等）
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetMembersAsync(Guid id, SessionUnitGetListDestinationInput input)
    {
        var entity = await Repository.GetAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled)
            .WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            .WhereIf(input.IsStatic.HasValue, x => x.Setting.IsStatic == input.IsStatic)
            .WhereIf(input.IsPublic.HasValue, x => x.Setting.IsPublic == input.IsPublic)
            .WhereIf(input.IsMuted == true, x => x.Setting.MuteExpireTime != null && x.Setting.MuteExpireTime <= Clock.Now)
            .WhereIf(input.IsMuted == false, x => x.Setting.MuteExpireTime == null || x.Setting.MuteExpireTime > Clock.Now)
            .WhereIf(input.OwnerIdList.IsAny(), x => input.OwnerIdList.Contains(x.OwnerId))
            .WhereIf(input.OwnerTypeList.IsAny(), x => input.OwnerTypeList.Contains(x.Owner.ObjectType.Value))
            .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(x => x.SessionTagId == input.TagId))
            .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(x => x.SessionRoleId == input.RoleId))
            .WhereIf(!input.JoinWay.IsEmpty(), x => x.Setting.JoinWay == input.JoinWay)
            .WhereIf(!input.InviterId.IsEmpty(), x => x.Setting.InviterId == input.InviterId)
            //排除自已
            .WhereIf(entity.DestinationObjectType != ChatObjectTypeEnums.Room, x => x.Id != id)
            //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Owner.Title.Contains(input.Keyword))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input, q => q.OrderByDescending(x => x.CreationTime));
    }

    /// <summary>
    /// 获取目标OwnerId列表（好友、群、群成员、机器人等）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<long>> GetListDestinationOwnerIdListAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        var list = await SessionUnitManager.GetMembersAsync(entity.SessionId.Value);

        return list.Select(x => x.OwnerId).ToList();

        //var ownerIdList = (await Repository.GetQueryableAsync())
        //    .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled)
        //    .Where(x => x.Setting.IsKilled == false)
        //    .Where(x => x.Setting.IsPublic == true)
        //    .Select(x => x.OwnerId)
        //    .Distinct()
        //    .ToList();

        //return ownerIdList;
    }

    /// <summary>
    /// 获取目标名称列表
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDisplayNameDto>> GetListDestinationNamesAsync(Guid id, GetListInput input)
    {
        var entity = await Repository.GetAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId)
            .Where(x => x.Setting.IsPublic)
            .Where(SessionUnit.GetActivePredicate(Clock.Now))
            .Select(x => new
            {
                x.Id,
                x.Setting.MemberName,
                x.Setting.Rename,
                x.Owner.Name,
            })
            .Select(x => new SessionUnitDisplayNameDto
            {
                Id = x.Id,
                DisplayName = !string.IsNullOrEmpty(x.MemberName)
                    ? x.MemberName
                    : (!string.IsNullOrEmpty(x.Rename) ? x.Rename : x.Name),
            })
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.DisplayName.Contains(input.Keyword));

        return await GetPagedListAsync(query, input);
    }

    /// <summary>
    /// 获取一个会话单元
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitOwnerDto> GetAsync(Guid id)
    {
        var entity = await GetEntityAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetPolicyName));

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 获取一个会话单元(缓存)
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitCacheItem> GetByCacheAsync(Guid id)
    {
        //var entity = await GetEntityAsync(id);

        //await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetPolicyName));

        return await SessionUnitManager.GetCacheAsync(id);
    }

    /// <summary>
    /// 获取多个个会话单元
    /// </summary>
    /// <param name="idList"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<PagedResultDto<SessionUnitOwnerDto>> GetManyAsync(List<Guid> idList)
    {
        var items = new List<SessionUnitOwnerDto>();

        foreach (var id in idList.Distinct())
        {
            var entity = await GetEntityAsync(id);

            await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetPolicyName));

            items.Add(await MapToDtoAsync(entity));
        }
        return new PagedResultDto<SessionUnitOwnerDto>(items.Count, items);
    }

    /// <summary>
    /// 获取一个会话单元（详情）
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitDetailDto> GetDetailAsync(Guid id)
    {
        var entity = await GetEntityAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetDetailPolicyName));

        var result = ObjectMapper.Map<SessionUnit, SessionUnitDetailDto>(entity);

        result.SessionUnitCount = await SessionUnitManager.GetCountBySessionIdAsync(entity.SessionId.Value);

        return result;
    }



    /// <summary>
    /// 获取一个会话单元（目标）
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <param name="destinationId">目标会话单元Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitDestinationDto> GetDestinationAsync([Required] Guid id, [Required] Guid destinationId)
    {
        var entity = await GetEntityAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetItemForSameSessionPolicyName));

        var destinationEntity = await GetEntityAsync(destinationId);

        Assert.If(entity.SessionId != destinationEntity.SessionId, $"Not in the same session");

        //var friendEntity = await SessionUnitManager.FindAsync(entity.OwnerId, destinationEntity.OwnerId);

        //if (friendEntity != null)
        //{
        //    return await MapToDestinationDtoAsync(friendEntity);
        //}

        return await MapToDestinationDtoAsync(destinationEntity);
    }

    /// <summary>
    /// 获取相同聊天对象的会话单元列表(共同好友、群、聊天广场等)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetListSameDestinationAsync(SessionUnitGetListSameDestinationInput input)
    {
        await CheckPolicyForUserAsync(new[] { input.SourceId, input.TargetId }, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        var query = (await SessionUnitManager.GetSameDestinationQeuryableAsync(input.SourceId, input.TargetId, input.ObjectTypeList))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Destination.Name.Contains(input.Keyword))
            ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input, x => x.OrderByDescending(x => x.Id));
    }

    /// <summary>
    /// 获取相同的会话单元列表(好友、共同好友、群、聊天广场等)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetListSameSessionAsync(SessionUnitGetListSameSessionInput input)
    {
        await CheckPolicyForUserAsync(new[] { input.SourceId, input.TargetId }, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        var query = (await SessionUnitManager.GetSameSessionQeuryableAsync(input.SourceId, input.TargetId, input.ObjectTypeList))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Destination.Name.Contains(input.Keyword))
            ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input, x => x.OrderByDescending(x => x.Id));
    }

    /// <summary>
    /// 获取相同的会话单元数量(好友、共同好友、群、聊天广场等)
    /// </summary>
    /// <param name="sourceId">原聊天对象Id</param>
    /// <param name="targetId">目标聊天对象Id</param>
    /// <param name="objectTypeList">聊天对象类型</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<int> GetSameSessionCountAsync([Required] long sourceId, [Required] long targetId, List<ChatObjectTypeEnums> objectTypeList)
    {
        await CheckPolicyForUserAsync(new[] { sourceId, targetId }, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        return await SessionUnitManager.GetSameSessionCountAsync(sourceId, targetId, objectTypeList);
    }

    /// <summary>
    /// 获取相同聊天对象的会话单元数量(共同好友、群、聊天广场等)
    /// </summary>
    /// <param name="sourceId">原聊天对象Id</param>
    /// <param name="targetId">目标聊天对象Id</param>
    /// <param name="objectTypeList">聊天对象类型</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<int> GetSameDestinationCountAsync([Required] long sourceId, [Required] long targetId, List<ChatObjectTypeEnums> objectTypeList)
    {
        await CheckPolicyForUserAsync(new[] { sourceId, targetId }, () => CheckPolicyAsync(GetListForSameSessionPolicyName));

        return await SessionUnitManager.GetSameDestinationCountAsync(sourceId, targetId, objectTypeList);
    }

    /// <inheritdoc/>
    protected virtual Task<SessionUnitOwnerDto> MapToDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(entity));
    }

    /// <inheritdoc/>
    protected virtual Task<SessionUnitDestinationDto> MapToDestinationDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDestinationDto>(entity));
    }

    /// <summary>
    /// 获取消息角标（新消息）- ownerId
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <param name="isImmersed">是否包含免打扰的消息</param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<Dictionary<ChatObjectTypeEnums, int>> GetTypedBadgeByOwnerIdAsync([Required] long ownerId, bool? isImmersed = null)
    {
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetBadgePolicyName));

        var badges = await SessionUnitManager.GetTypeBadgeByOwnerIdAsync(ownerId, isImmersed);

        var chatObjectInfo = await ChatObjectManager.GetItemByCacheAsync(ownerId);

        return badges;
    }

    /// <summary>
    /// 获取消息角标（新消息）- ownerId
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <param name="isImmersed">是否包含免打扰的消息</param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<BadgeDto> GetBadgeByOwnerIdAsync([Required] long ownerId, bool? isImmersed = null)
    {
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetBadgePolicyName));

        var badge = await SessionUnitManager.GetBadgeByOwnerIdAsync(ownerId, isImmersed);

        var chatObjectInfo = await ChatObjectManager.GetItemByCacheAsync(ownerId);

        return new BadgeDto()
        {
            AppUserId = chatObjectInfo?.AppUserId,
            ChatObjectId = ownerId,
            Badge = badge
        };
    }

    /// <summary>
    /// 获取消息角标（新消息）- sessionUnitId
    /// </summary>
    /// <param name="id">会话单元Id</param>
    /// <param name="isImmersed">是否包含免打扰的消息</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<BadgeDto> GetBadgeByIdAsync([Required] Guid id, bool? isImmersed = null)
    {
        var entity = await GetEntityAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetBadgePolicyName));

        var badge = await SessionUnitManager.GetBadgeByIdAsync(id, isImmersed);

        return new BadgeDto()
        {
            AppUserId = entity.Owner.AppUserId,
            ChatObjectId = entity.OwnerId,
            Badge = badge
        };
    }

    /// <summary>
    /// 获取用户总的消息角标（新消息）- userId
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="isImmersed">是否包含免打扰的消息</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId, bool? isImmersed = null)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(userId);

        await CheckPolicyForUserAsync(chatObjectIdList, () => CheckPolicyAsync(GetBadgePolicyName));

        var dic = await SessionUnitManager.GetBadgeByOwnerIdListAsync(chatObjectIdList, isImmersed);

        var result = dic.Select(x => new BadgeDto()
        {
            AppUserId = userId,
            ChatObjectId = x.Key,
            Badge = x.Value,

        }).ToList();

        //var result = new List<BadgeDto>();

        //foreach (var chatObjectId in chatObjectIdList)
        //{
        //    result.Add(await GetBadgeByOwnerIdAsync(chatObjectId, isImmersed));
        //}
        return result;
    }

    /// <summary>
    /// 获取当前用户总的消息角标（新消息）
    /// </summary>
    /// <param name="isImmersed">是否包含免打扰的消息</param>
    /// <returns></returns>
    [HttpGet]
    public Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null)
    {
        return GetBadgeByUserIdAsync(Assert.NotNull(CurrentUser.GetId(), "未登录"), isImmersed);
    }

    /// <summary>
    /// 查询会话单元
    /// </summary>
    /// <param name="ownerId">原聊天对象Id</param>
    /// <param name="destinactionId">目标聊天对象Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<Guid> FindIdAsync([Required] long ownerId, [Required] long destinactionId)
    {
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(FindPolicyName));

        var entity = await SessionUnitManager.FindAsync(ownerId, destinactionId);

        Assert.NotNull(entity, "No found!");

        return entity.Id;
    }

    /// <summary>
    /// 获取会话单元列表【缓存】
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitCacheItem>> GetListCachesAsync(SessionUnitCacheGetListInput input)
    {
        Assert.If(input.SessionUnitId == null && input.SessionId == null, "SessionUnitId Or SessionId cannot both be null");

        var sessionId = input.SessionId;

        if (input.SessionUnitId.HasValue)
        {
            var entity = await GetEntityAsync(input.SessionUnitId.Value);

            await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetListPolicyName));

            Assert.If(sessionId.HasValue && sessionId != entity.SessionId.Value, "Not in the same session");

            sessionId = entity.SessionId.Value;
        }
        else
        {
            await CheckPolicyAsync(GetListPolicyName);
        }

        var items = await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId.Value);

        var query = items.AsQueryable()
            .WhereIf(input.SessionUnitId.HasValue, x => x.Id == input.SessionUnitId);

        return await GetPagedListAsync<SessionUnitCacheItem, SessionUnitCacheItem>(query, input);
    }

    /// <summary>
    /// 获取一个会话单元【缓存】
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionUnitCacheItem> GetCacheAsync([Required] Guid sessionUnitId)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);

        await CheckPolicyForUserAsync(unit.OwnerId, () => CheckPolicyAsync(GetPolicyName));

        return unit;
    }

    /// <summary>
    /// 获取会话单元记数器（新消息，提醒、@我、@所有人等）
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionUnitCounterInfo> GetCounterAsync(SessionUnitGetCounterInput input)
    {
        var entity = await GetEntityAsync(input.SessionUnitId);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetCounterPolicyName));

        return await SessionUnitManager.GetCounterAsync(input.SessionUnitId, input.MinMessageId, input.IsImmersed);
    }

    /// <summary>
    /// 根据创建用户创建相应的会话(通知\新闻\机器人等)
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<int> GenerateDefaultSessionAsync(long chatObjectId)
    {
        var userChatObject = await ChatObjectManager.GetAsync(chatObjectId);

        var sessionUnitList = await SessionUnitManager.GenerateDefaultSessionByChatObjectAsync(userChatObject);

        return sessionUnitList.Count;
    }

    /// <summary>
    /// 更新会话单元Ticks
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="ticks"></param>
    /// <returns></returns>

    [HttpPost]
    public async Task<long> UpdateTicksAsync(Guid sessionUnitId, long? ticks)
    {
        return await Repository.UpdateTicksAsync(sessionUnitId, ticks);
    }

    /// <summary>
    /// 清除消息角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<long> ClearBadgeAsync(long ownerId)
    {

        var result = await Repository.ClearBadgeAsync(ownerId);
        var clearResult = await SessionUnitCacheManager.ClearBadgeAsync(ownerId);
        return result;
    }

    /// <summary>
    /// 设置会话盒子
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="boxId"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitCacheItem> SetBoxAsync([Required] Guid sessionUnitId, [Required] Guid boxId)
    {
        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);

        await CheckPolicyForUserAsync(unit.OwnerId, () => CheckPolicyAsync(GetPolicyName));

        await SessionUnitManager.SetBoxAsync(unit.Id, boxId);

        return unit;
    }
}
