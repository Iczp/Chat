using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.Announcements;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.RoomSections.Rooms;

/// <summary>
/// 群
/// </summary>
public class RoomAppService(IRoomManager roomManager,
    IChatObjectCategoryManager chatObjectCategoryManager,
    ISessionPermissionChecker sessionPermissionChecker) : ChatAppService, IRoomAppService
{
    public virtual string InvitePolicyName { get; set; }
    public virtual string CreateRoomPolicyName { get; set; }
    protected IRoomManager RoomManager { get; } = roomManager;
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; } = chatObjectCategoryManager;
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IChatObjectRepository ChatObjectRepository => LazyServiceProvider.LazyGetRequiredService<IChatObjectRepository>();
    protected ISessionUnitRepository SessionUnitRepository => LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();

    /// <summary>
    /// 创建群
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateAsync(RoomCreateInput input)
    {
        await CheckPolicyAsync(CreateRoomPolicyName);

        var entity = await RoomManager.CreateAsync(input.Name, input.ChatObjectIdList, input.OwnerId);

        return await MapToChatObjectDtoAsync(entity);
    }

    protected virtual Task<ChatObjectDto> MapToChatObjectDtoAsync(ChatObject chatObject)
    {
        return MapToChatObjectDto(chatObject);
    }

    protected virtual Task<ChatObjectDto> MapToChatObjectDto(ChatObject chatObject)
    {
        return Task.FromResult(ObjectMapper.Map<ChatObject, ChatObjectDto>(chatObject));
    }

    /// <summary>
    /// 邀请加入群聊
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<List<SessionUnitSenderInfo>> InviteAsync(InviteInput input)
    {
        await CheckPolicyAsync(InvitePolicyName);

        var list = await RoomManager.InviteAsync(input, autoSendMessage: true);

        return ObjectMapper.Map<List<SessionUnit>, List<SessionUnitSenderInfo>>(list);
    }

    /// <summary>
    /// 获取共同所在的群列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetSameAsync(SameGetListInput input)
    {
        var query = await SessionUnitManager.GetSameSessionQeuryableAsync(input.SourceChatObjectId, input.TargetChatObjectId, [ChatObjectTypeEnums.Room]);

        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input);
    }

    /// <summary>
    /// 获取共同所在的群数量
    /// </summary>
    /// <param name="sourceChatObjectId">原聊天对象Id</param>
    /// <param name="targetChatObjectId">目标对象Id</param>
    /// <returns></returns>
    [HttpGet]
    public Task<int> GetSameCountAsync(long sourceChatObjectId, long targetChatObjectId)
    {
        return SessionUnitManager.GetSameSessionCountAsync(sourceChatObjectId, targetChatObjectId, new List<ChatObjectTypeEnums>() { ChatObjectTypeEnums.Room });
    }

    /// <summary>
    /// 更新群名称并群内公告
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="name">群名字</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ChatObjectDto> UpdateNameAsync(Guid sessionUnitId, string name)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        //await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName, sessionUnit);

        var entity = await RoomManager.UpdateNameAsync(sessionUnit, name);

        return await MapToChatObjectDtoAsync(entity);
    }

    /// <summary>
    /// 修改群头像
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="thubmnail"></param>
    /// <param name="portrait">头像,全地址或相对地址:"http://www.icpz.net/logo.png","/logo.png"</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> UpdatePortraitAsync(Guid sessionUnitId, string thubmnail, string portrait)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdatePortrait, sessionUnit);

        var entity = await RoomManager.UpdatePortraitAsync(sessionUnit, thubmnail, portrait);

        return await MapToChatObjectDtoAsync(entity);
    }

    /// <summary>
    /// 设置加群验证方式（群主或具备申请审批权限的管理员）
    /// </summary>
    [HttpPost]
    public virtual async Task<ChatObjectDto> SetVerificationMethodAsync(Guid sessionUnitId, VerificationMethods verificationMethod)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        await EnsureCurrentUserOwnsUnitAsync(sessionUnit);

        if (sessionUnit.Setting?.IsCreator != true && sessionUnit.Setting?.IsAdmin != true)
        {
            await SessionPermissionChecker.CheckAsync(
                SessionPermissionDefinitionConsts.SessionRequestPermission.Handle,
                sessionUnit);
        }

        var room = await ChatObjectManager.GetAsync(sessionUnit.DestinationId!.Value);
        room.SetVerificationMethod(verificationMethod);
        await ChatObjectRepository.UpdateAsync(room, autoSave: true);

        return await MapToChatObjectDtoAsync(room);
    }

    /// <summary>
    /// 转让群主
    /// </summary>
    [HttpPost]
    public virtual async Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);
        await EnsureCurrentUserOwnsUnitAsync(sessionUnit);
        await RoomManager.TransferCreatorAsync(sessionUnitId, targetSessionUnitId);
    }

    /// <summary>
    /// 当群内无群主且无管理员时，当前有效群成员认领成为群主
    /// </summary>
    [HttpPost]
    public virtual async Task<RoomManagementSettingsDto> ClaimCreatorAsync(Guid sessionUnitId)
    {
        var sessionUnit = await GetDetailedSessionUnitAsync(sessionUnitId);
        await EnsureCurrentUserOwnsUnitAsync(sessionUnit);
        await RoomManager.ClaimCreatorAsync(sessionUnitId);
        return await GetManagementSettingsAsync(sessionUnitId);
    }

    /// <summary>
    /// 解散群（仅群主）
    /// </summary>
    [HttpPost]
    public virtual async Task DissolveAsync(Guid sessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);
        await EnsureCurrentUserOwnsUnitAsync(sessionUnit);
        Assert.If(sessionUnit.Setting?.IsCreator != true, "仅群主可解散群聊");

        if (sessionUnit.SessionId.HasValue)
        {
            var sessionRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
            var session = await sessionRepository.GetAsync(sessionUnit.SessionId.Value);
            session.SetIsEnabled(false);
            await sessionRepository.UpdateAsync(session, autoSave: true);
        }
    }

    /// <summary>
    /// 获取群管理中心配置与当前成员管理权限菜单
    /// </summary>
    [HttpGet]
    public virtual async Task<RoomManagementSettingsDto> GetManagementSettingsAsync(Guid sessionUnitId)
    {
        var sessionUnit = await GetDetailedSessionUnitAsync(sessionUnitId);
        await EnsureCurrentUserOwnsUnitAsync(sessionUnit);

        Assert.If(!sessionUnit.SessionId.HasValue || !sessionUnit.DestinationId.HasValue, "无效的群会话单元");

        var sessionRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        var session = await sessionRepository.GetAsync(sessionUnit.SessionId.Value);
        var room = await ChatObjectRepository.GetAsync(sessionUnit.DestinationId.Value);

        var isCreator = sessionUnit.Setting?.IsCreator == true || sessionUnit.Setting?.IsStatic == true;
        var isAdmin = sessionUnit.IsAdmin;
        var isAdminOrCreator = isCreator || isAdmin;

        var canManageSettings = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName,
            SessionPermissionDefinitionConsts.ChatObjectPermission.UpdatePortrait,
            SessionPermissionDefinitionConsts.ChatObjectPermission.ToggleAllInputEnabled,
            SessionPermissionDefinitionConsts.ChatObjectPermission.VerificationMethod);

        var canManageJoinRequests = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.SessionRequestPermission.Handle);

        var canManageAnnouncements = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Create,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Publish,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Approve,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Update,
            SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn,
            SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Delete,
            SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteOwn,
            SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Pin,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Remind,
            SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat);

        var canApproveAnnouncements = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.AnnouncementPermission.Approve);

        var canManageMembers = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.SessionUnitPermissions.RemoveMember,
            SessionPermissionDefinitionConsts.SessionUnitPermissions.SetMuteExpireTime,
            SessionPermissionDefinitionConsts.SessionUnitPermissions.ToggleInputEnabled);

        var canManageOrgs = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create,
            SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update,
            SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete);

        var canManageRoles = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.SessionRolePermission.Create,
            SessionPermissionDefinitionConsts.SessionRolePermission.Update,
            SessionPermissionDefinitionConsts.SessionRolePermission.Delete,
            SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions);

        var canManagePermissions = isAdminOrCreator || await IsAnyPermissionGrantedAsync(
            sessionUnit,
            SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions);

        var canTransferOrDissolve = isCreator;

        var activeUnitQuery = (await SessionUnitRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id && x.Setting != null && x.Setting.IsEnabled && !x.Setting.IsKilled);
        var hasCreator = isCreator || await AsyncExecuter.AnyAsync(activeUnitQuery.Where(x => x.Setting.IsCreator));
        var hasAdmin = isAdmin || await AsyncExecuter.AnyAsync(
            activeUnitQuery.Where(x => x.SessionUnitRoleList.Any(r =>
                r.SessionRole != null && (r.SessionRole.Name == "管理员" || r.SessionRole.Name == "Admin"))));
        var canClaimCreator = !hasCreator
            && !hasAdmin
            && session.IsEnabled
            && sessionUnit.Setting != null
            && sessionUnit.Setting.IsEnabled
            && !sessionUnit.Setting.IsKilled;

        var pendingJoinRequestCount = 0;
        if (canManageJoinRequests)
        {
            var reqRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionRequest, Guid>>();
            var reqQuery = (await reqRepo.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => x.DestinationId == room.Id && !x.IsHandled && x.IsEnabled)
                .Where(x => x.ExpirationTime == null || x.ExpirationTime > Clock.Now);
            pendingJoinRequestCount = await AsyncExecuter.CountAsync(reqQuery);
        }

        var pendingAnnouncementCount = 0;
        if (canApproveAnnouncements)
        {
            var annRepo = LazyServiceProvider.LazyGetRequiredService<ISessionAnnouncementRepository>();
            var annQuery = (await annRepo.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => x.SessionId == session.Id && x.Status == AnnouncementStatus.PendingReview);
            pendingAnnouncementCount = await AsyncExecuter.CountAsync(annQuery);
        }

        return new RoomManagementSettingsDto
        {
            SessionId = session.Id,
            RoomId = room.Id,
            Name = room.Name,
            Portrait = room.Portrait,
            Description = room.Description,
            VerificationMethod = room.VerificationMethod,
            IsAllMuted = session.IsAllMuted,
            InvitePolicy = session.InvitePolicy,
            AllowJoinChannels = session.AllowJoinChannels,
            PendingJoinRequestCount = pendingJoinRequestCount,
            PendingAnnouncementCount = pendingAnnouncementCount,
            IsCreator = isCreator,
            IsAdmin = isAdmin,
            HasCreator = hasCreator,
            HasAdmin = hasAdmin,
            CanClaimCreator = canClaimCreator,
            CanEnterManagement = canManageSettings
                || canManageJoinRequests
                || canManageAnnouncements
                || canManageMembers
                || canManageOrgs
                || canManageRoles
                || canManagePermissions
                || canTransferOrDissolve,
            CanManageSettings = canManageSettings,
            CanManageJoinRequests = canManageJoinRequests,
            CanManageAnnouncements = canManageAnnouncements,
            CanManageMembers = canManageMembers,
            CanManageOrgs = canManageOrgs,
            CanManageRoles = canManageRoles,
            CanManagePermissions = canManagePermissions,
            CanTransferOrDissolve = canTransferOrDissolve,
        };
    }

    /// <summary>
    /// 统一更新群策略（入群审核方式、全员禁言、邀请策略、允许入群渠道、群简介）
    /// </summary>
    [HttpPost]
    public virtual async Task<RoomManagementSettingsDto> UpdatePoliciesAsync(UpdateRoomPoliciesInput input)
    {
        var sessionUnit = await GetDetailedSessionUnitAsync(input.SessionUnitId);
        await EnsureCurrentUserOwnsUnitAsync(sessionUnit);

        var isAdminOrCreator = sessionUnit.Setting?.IsCreator == true
            || sessionUnit.Setting?.IsStatic == true
            || sessionUnit.Setting?.IsAdmin == true;

        if (!isAdminOrCreator)
        {
            var hasPolicyPerm = await IsAnyPermissionGrantedAsync(
                sessionUnit,
                SessionPermissionDefinitionConsts.ChatObjectPermission.ToggleAllInputEnabled,
                SessionPermissionDefinitionConsts.ChatObjectPermission.VerificationMethod,
                SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName,
                SessionPermissionDefinitionConsts.SessionRequestPermission.Handle);
            if (!hasPolicyPerm)
            {
                throw new AbpAuthorizationException("无权修改群管理策略");
            }
        }

        Assert.If(!sessionUnit.SessionId.HasValue || !sessionUnit.DestinationId.HasValue, "无效的群会话单元");

        var sessionRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        var session = await sessionRepository.GetAsync(sessionUnit.SessionId.Value);
        var room = await ChatObjectRepository.GetAsync(sessionUnit.DestinationId.Value);

        var roomChanged = false;
        if (input.VerificationMethod.HasValue && room.VerificationMethod != input.VerificationMethod.Value)
        {
            room.SetVerificationMethod(input.VerificationMethod.Value);
            roomChanged = true;
        }
        if (input.Description != null && room.Description != input.Description)
        {
            room.Description = input.Description;
            roomChanged = true;
        }
        if (roomChanged)
        {
            await ChatObjectRepository.UpdateAsync(room, autoSave: true);
        }

        session.SetRoomPolicies(input.IsAllMuted, input.InvitePolicy, input.AllowJoinChannels);
        await sessionRepository.UpdateAsync(session, autoSave: true);

        return await GetManagementSettingsAsync(sessionUnit.Id);
    }

    protected virtual async Task<SessionUnit> GetDetailedSessionUnitAsync(Guid sessionUnitId)
    {
        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Setting)
            .Include(x => x.GrantList)
            .Include(x => x.SessionUnitRoleList)
                .ThenInclude(x => x.SessionRole)
                    .ThenInclude(x => x.GrantList);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == sessionUnitId);
        return Assert.NotNull(entity, $"SessionUnit not found: {sessionUnitId}");
    }

    protected virtual async Task EnsureCurrentUserOwnsUnitAsync(SessionUnit sessionUnit)
    {
        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            if (!ownedIds.Contains(sessionUnit.OwnerId))
            {
                throw new AbpAuthorizationException("无权操作该群会话设置");
            }
        }
    }

    protected virtual async Task<bool> IsAnyPermissionGrantedAsync(SessionUnit sessionUnit, params string[] permissionIds)
    {
        foreach (var permissionId in permissionIds)
        {
            if (await SessionPermissionChecker.IsGrantedAsync(permissionId, sessionUnit))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 允许加入群的聊天对象类型
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<PagedResultDto<EnumDto>> GetListAllowJoinTypes()
    {
        await Task.Yield();

        var items = ChatConsts.AllowJoinRoomObjectTypes.Select(x => new EnumDto()
        {
            Name = x.ToString(),
            Value = (int)x,
            Description = x.GetDescription()
        }).ToList();

        return new PagedResultDto<EnumDto>(items.Count, items);
    }
}
