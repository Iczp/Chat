using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话单元
/// </summary>
public class SessionUnitAppService : ChatAppService, ISessionUnitAppService
{
    protected override string GetListPolicyName { get; set; }
    protected override string GetPolicyName { get; set; }
    protected virtual string SetRenamePolicyName { get; set; }
    protected virtual string SetMemberNamePolicyName { get; set; }
    protected virtual string GetDetailPolicyName { get; set; }
    protected virtual string SetReadedPolicyName { get; set; }
    protected virtual string SetToppingPolicyName { get; set; }
    protected virtual string SetImmersedPolicyName { get; set; }
    protected virtual string RemoveSessionPolicyName { get; set; }
    protected virtual string ClearMessagePolicyName { get; set; }
    protected virtual string DeleteMessagePolicyName { get; set; }

    protected ISessionUnitRepository Repository { get; }
    protected IMessageRepository MessageRepository { get; }
    protected IReadedRecorderManager ReadedRecorderManager { get; }
    protected IOpenedRecorderManager OpenedRecorderManager { get; }
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; }
    protected IFollowManager FollowManager { get; }

    public SessionUnitAppService(
        IMessageRepository messageRepository,
        ISessionUnitRepository repository,
        IReadedRecorderManager readedRecorderManager,
        IOpenedRecorderManager openedRecorderManager,
        IFavoritedRecorderManager favoriteManager,
        IFollowManager followManager)
    {
        MessageRepository = messageRepository;
        Repository = repository;
        ReadedRecorderManager = readedRecorderManager;
        OpenedRecorderManager = openedRecorderManager;
        FavoritedRecorderManager = favoriteManager;
        FollowManager = followManager;
    }

    /// <inheritdoc/>
    protected override Task CheckPolicyAsync(string policyName)
    {
        return base.CheckPolicyAsync(policyName);
    }


    /// <inheritdoc/>
    protected virtual async Task<SessionUnit> GetEntityAsync(Guid id, bool checkIsKilled = true)
    {
        var entity = await Repository.GetAsync(id);

        Assert.If(checkIsKilled && entity.Setting.IsKilled, "已经删除的会话单元!");

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
            .WhereIf(input.MinMessageId.HasValue, x => x.LastMessageId > input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.LastMessageId <= input.MaxMessageId)
            .WhereIf(input.IsTopping == true, x => x.Sorting > 0)
            .WhereIf(input.IsTopping == false, x => x.Sorting == 0)
            .WhereIf(input.IsContacts.HasValue, x => x.Setting.IsContacts == input.IsContacts)
            .WhereIf(input.IsImmersed.HasValue, x => x.Setting.IsImmersed == input.IsImmersed)
            .WhereIf(input.IsBadge.HasValue, x => x.PublicBadge > 0 || x.PrivateBadge > 0)
            .WhereIf(input.IsRemind.HasValue, x => x.RemindAllCount > 0 || x.RemindMeCount > 0)
            .WhereIf(input.IsFollowing.HasValue, x => x.FollowingCount > 0)
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
        await CheckPolicyAsync(GetListPolicyName);

        var query = await CreateQueryAsync(input);

        return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
            query,
            input,
            x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
    }

    /// <summary>
    /// 消息统计
    /// </summary>
    /// <param name="idList"></param>
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
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListDestinationAsync(Guid id, SessionUnitGetListDestinationInput input)
    {
        var entity = await Repository.GetAsync(id);

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled)
            .WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            .WhereIf(input.IsStatic.HasValue, x => x.Setting.IsStatic == input.IsStatic)
            .WhereIf(input.IsPublic.HasValue, x => x.Setting.IsPublic == input.IsPublic)
            .WhereIf(input.OwnerIdList.IsAny(), x => input.OwnerIdList.Contains(x.OwnerId))
            .WhereIf(input.OwnerTypeList.IsAny(), x => input.OwnerTypeList.Contains(x.Owner.ObjectType.Value))
            .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(x => x.SessionTagId == input.TagId))
            .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(x => x.SessionRoleId == input.RoleId))
            .WhereIf(!input.JoinWay.IsEmpty(), x => x.Setting.JoinWay == input.JoinWay)
            .WhereIf(!input.InviterId.IsEmpty(), x => x.Setting.InviterId == input.InviterId)
            //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Owner.Name.Contains(input.Keyword))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input, q => q.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
    }

    /// <summary>
    /// 获取目标名称列表
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDisplayName>> GetListDestinationNamesAsync(Guid id, GetListInput input)
    {
        var entity = await Repository.GetAsync(id);

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled)
            .Where(SessionUnit.GetActivePredicate())
            .Select(x => new SessionUnitDisplayName
            {
                Id = x.Id,
                DisplayName = !string.IsNullOrEmpty(x.Setting.MemberName) ? x.Setting.MemberName : x.Setting.Rename,
            })
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.DisplayName.Contains(input.Keyword));

        return await GetPagedListAsync<SessionUnitDisplayName, SessionUnitDisplayName>(query, input);
    }

    /// <summary>
    /// 获取一个会话单元
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitOwnerDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(GetPolicyName);

        var entity = await GetEntityAsync(id);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 获取一个会话单元（详情）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitOwnerDetailDto> GetDetailAsync(Guid id)
    {
        await CheckPolicyAsync(GetDetailPolicyName);

        var entity = await GetEntityAsync(id);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDetailDto>(entity);
    }

    /// <summary>
    /// 获取一个会话单元（目标）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="destinationId"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionUnitDestinationDto> GetDestinationAsync(Guid id, Guid destinationId)
    {
        var destinationEntity = await GetEntityAsync(destinationId);

        var selfEntity = await GetEntityAsync(id);

        Assert.If(selfEntity.SessionId != destinationEntity.SessionId, $"Not in the same session");

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
        var query = (await SessionUnitManager.GetSameSessionQeuryableAsync(input.SourceId, input.TargetId, input.ObjectTypeList))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Destination.Name.Contains(input.Keyword))
            ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input, x => x.OrderByDescending(x => x.Id));
    }

    /// <summary>
    /// 获取相同的会话单元数量(好友、共同好友、群、聊天广场等)
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="targetId"></param>
    /// <param name="objectTypeList"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<int> GetSameSessionCountAsync(long sourceId, long targetId, List<ChatObjectTypeEnums> objectTypeList)
    {
        return SessionUnitManager.GetSameSessionCountAsync(sourceId, targetId, objectTypeList);
    }

    /// <summary>
    /// 获取相同聊天对象的会话单元数量(共同好友、群、聊天广场等)
    /// </summary>
    /// <param name="sourceId">原聊天对象Id</param>
    /// <param name="targetId">目标聊天对象Id</param>
    /// <param name="objectTypeList">聊天对象类型</param>
    /// <returns></returns>
    [HttpGet]
    public Task<int> GetSameDestinationCountAsync(long sourceId, long targetId, List<ChatObjectTypeEnums> objectTypeList)
    {
        return SessionUnitManager.GetSameDestinationCountAsync(sourceId, targetId, objectTypeList);
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
    public async Task<BadgeDto> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null)
    {
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
    public async Task<BadgeDto> GetBadgeByIdAsync(Guid id, bool? isImmersed = null)
    {
        var entity = await GetEntityAsync(id);

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
    public async Task<List<BadgeDto>> GetBadgeByUserIdAsync(Guid userId, bool? isImmersed = null)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserId(userId);

        var result = new List<BadgeDto>();

        foreach (var chatObjectId in chatObjectIdList)
        {
            result.Add(await GetBadgeByOwnerIdAsync(chatObjectId, isImmersed));
        }
        return result;
    }

    /// <summary>
    /// 获取当前用户总的消息角标（新消息）
    /// </summary>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null)
    {
        return GetBadgeByUserIdAsync(CurrentUser.GetId(), isImmersed);
    }

    /// <summary>
    /// 查询会话单元
    /// </summary>
    /// <param name="ownerId">原聊天对象Id</param>
    /// <param name="destinactionId">目标聊天对象Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<Guid> FindIdAsync(long ownerId, long destinactionId)
    {
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

            Assert.If(sessionId.HasValue && sessionId != entity.SessionId.Value, "Not in the same session");

            sessionId = entity.SessionId.Value;
        }

        var items = await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId.Value);

        var query = items.AsQueryable()
            .WhereIf(input.SessionUnitId.HasValue, x => x.Id == input.SessionUnitId);

        return await GetPagedListAsync<SessionUnitCacheItem, SessionUnitCacheItem>(query, input);
    }

    /// <summary>
    /// 获取一个会话单元【缓存】
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionUnitCacheItem> GetCacheAsync(Guid sessionUnitId)
    {
        var entity = await GetEntityAsync(sessionUnitId);

        return await SessionUnitManager.GetCacheItemAsync(entity);
    }

    /// <summary>
    /// 获取会话单元记数器（新消息，提醒、@我、@所有人等）
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="minMessageId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionUnitCounterInfo> GetCounterAsync(Guid sessionUnitId, long minMessageId = 0, bool? isImmersed = null)
    {
        return await SessionUnitManager.GetCounterAsync(sessionUnitId, minMessageId, isImmersed);
    }   
}
