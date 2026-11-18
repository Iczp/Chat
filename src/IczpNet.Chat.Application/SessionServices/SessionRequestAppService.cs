using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话请求（申请添加好友、加入群聊）
/// </summary>
public class SessionRequestAppService
    : CrudChatAppService<
        SessionRequest,
        SessionRequestDetailDto,
        SessionRequestDto,
        Guid,
        SessionRequestGetListInput,
        SessionRequestCreateInput,
        SessionRequestUpdateInput>,
    ISessionRequestAppService
{
    protected override string GetListPolicyName { get; set; } = ChatPermissions.SessionRequestPermissions.GetAll;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.SessionRequestPermissions.Create;
    protected virtual string HandleRquestPolicyName { get; set; } = ChatPermissions.SessionRequestPermissions.HandleRquest;

    protected ISessionRequestManager SessionRequestManager { get; }
    protected ISessionPermissionChecker SessionPermissionChecker { get; }
    protected ISessionUnitRepository SessionUnitRepository => LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();

    public SessionRequestAppService(
        IRepository<SessionRequest, Guid> repository,
        ISessionRequestManager sessionRequestManager,
        ISessionPermissionChecker sessionPermissionChecker) : base(repository)
    {
        SessionRequestManager = sessionRequestManager;
        SessionPermissionChecker = sessionPermissionChecker;
    }

    protected override async Task<IQueryable<SessionRequest>> CreateFilteredQueryAsync(SessionRequestGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
            .WhereIf(input.IsExpired.HasValue, x => x.IsExpired == input.IsExpired)
            .WhereIf(input.IsHandled.HasValue, x => x.IsHandled == input.IsHandled)
            .WhereIf(input.IsAgreed.HasValue, x => x.IsAgreed == input.IsAgreed)
            .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
            .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime < input.EndCreationTime)
            .WhereIf(input.StartHandleTime.HasValue, x => x.HandleTime >= input.StartHandleTime)
            .WhereIf(input.StartHandleTime.HasValue, x => x.HandleTime < input.EndHandleTime)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.RequestMessage.Contains(input.Keyword) || x.HandleMessage.Contains(input.Keyword))
            ;
    }

    protected async Task<SessionUnit> FindAuthorizedRoomHandlerUnitAsync(long roomId, Guid? preferredSessionUnitId = null)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return null;
        }

        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (ownedIds.Count == 0)
        {
            return null;
        }

        if (preferredSessionUnitId.HasValue)
        {
            var preferred = await SessionUnitManager.GetAsync(preferredSessionUnitId.Value);
            if (preferred.DestinationId == roomId
                && ownedIds.Contains(preferred.OwnerId)
                && await SessionPermissionChecker.IsGrantedAsync(
                    SessionPermissionDefinitionConsts.SessionRequestPermission.Handle,
                    preferred))
            {
                return preferred;
            }
        }

        var query = await SessionUnitRepository.WithDetailsAsync(
            x => x.Setting,
            x => x.GrantList,
            x => x.SessionUnitRoleList);

        var candidates = await AsyncExecuter.ToListAsync(
            query.Where(x => x.DestinationId == roomId
                          && ownedIds.Contains(x.OwnerId)
                          && x.Setting.IsEnabled
                          && !x.Setting.IsKilled));

        foreach (var unit in candidates)
        {
            if (await SessionPermissionChecker.IsGrantedAsync(
                SessionPermissionDefinitionConsts.SessionRequestPermission.Handle,
                unit))
            {
                return unit;
            }
        }

        return null;
    }

    protected override async Task CheckGetListPolicyAsync(SessionRequestGetListInput input)
    {
        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            if ((input.OwnerId.HasValue && ownedIds.Contains(input.OwnerId.Value))
                || (input.DestinationId.HasValue && ownedIds.Contains(input.DestinationId.Value)))
            {
                return;
            }

            if (input.DestinationId.HasValue)
            {
                var dest = await ChatObjectManager.FindAsync(input.DestinationId.Value);
                if (dest?.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square)
                {
                    var handlerUnit = await FindAuthorizedRoomHandlerUnitAsync(dest.Id);
                    if (handlerUnit != null)
                    {
                        return;
                    }
                }
            }
        }

        await base.CheckGetListPolicyAsync(input);
    }

    [HttpGet]
    public override async Task<SessionRequestDetailDto> GetAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);

        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            var isParticipant = ownedIds.Contains(entity.OwnerId)
                || (entity.DestinationId.HasValue && ownedIds.Contains(entity.DestinationId.Value));

            if (!isParticipant && entity.DestinationId.HasValue)
            {
                var dest = await ChatObjectManager.FindAsync(entity.DestinationId.Value);
                if (dest?.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square)
                {
                    isParticipant = await FindAuthorizedRoomHandlerUnitAsync(dest.Id) != null;
                }
            }

            if (!isParticipant)
            {
                await CheckGetPolicyAsync();
            }
        }

        return await MapToGetOutputDtoAsync(entity);
    }

    protected override SessionRequest MapToEntity(SessionRequestCreateInput createInput)
    {
        return new SessionRequest(createInput.OwnerId, createInput.DestinationId, createInput.RequestMessage);
    }

    protected override async Task CheckCreateAsync(SessionRequestCreateInput input)
    {
        var entity = await SessionUnitManager.FindAsync(input.OwnerId, input.DestinationId);
        Assert.If(entity != null, "Already a friend");
    }

    /// <summary>
    /// 添加会话请求（加好友、加群、加聊天广场）
    /// </summary>
    [HttpPost]
    public override async Task<SessionRequestDetailDto> CreateAsync([FromQuery] SessionRequestCreateInput input)
    {
        await CheckPolicyForUserAsync(input.OwnerId, () => base.CheckCreateAsync(input));

        var entity = await SessionRequestManager.CreateRequestAsync(input.OwnerId, input.DestinationId, input.RequestMessage);

        return await MapToGetOutputDtoAsync(entity);
    }

    [HttpPost]
    [RemoteService(false)]
    public override Task<SessionRequestDetailDto> UpdateAsync(Guid id, SessionRequestUpdateInput input) => base.UpdateAsync(id, input);

    [HttpPost]
    [RemoteService(false)]
    public override Task DeleteAsync(Guid id) => base.DeleteAsync(id);

    [HttpPost]
    [RemoteService(false)]
    public override Task DeleteManyAsync(List<Guid> idList) => base.DeleteManyAsync(idList);

    /// <summary>
    /// 处理申请（支持个人好友申请与群主/管理员入群审批）
    /// </summary>
    [HttpPost]
    public async Task<SessionRequestDetailDto> HandleRequestAsync([FromQuery] SessionRequestHandleInput input)
    {
        var sessionRequest = await Repository.GetAsync(input.SessionRequestId);
        Guid? handlerSessionUnitId = input.HandlerSessionUnitId;

        if (sessionRequest.Destination?.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square)
        {
            if (CurrentUser.Id.HasValue)
            {
                var handlerUnit = await FindAuthorizedRoomHandlerUnitAsync(
                    sessionRequest.DestinationId!.Value,
                    input.HandlerSessionUnitId)
                    ?? throw new AbpAuthorizationException("仅群主或具备审核权限的管理员可处理入群申请");
                handlerSessionUnitId = handlerUnit.Id;
            }
        }
        else
        {
            await CheckPolicyForUserAsync(sessionRequest.DestinationId, () => CheckPolicyAsync(HandleRquestPolicyName));
        }

        var entity = await SessionRequestManager.HandleRequestAsync(
            input.SessionRequestId,
            input.IsAgreed,
            input.HandleMessage,
            handlerSessionUnitId);

        return await MapToGetOutputDtoAsync(entity);
    }
}
