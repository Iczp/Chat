﻿using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomAdminAppService : ChatAppService, IRoomAdminAppService
{
    public virtual string InvitePolicyName { get; set; }
    public virtual string CreateRoomPolicyName { get; set; }
    protected IRoomManager RoomManager { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
    protected ISessionPermissionChecker SessionPermissionChecker { get; }

    public RoomAdminAppService(IRoomManager roomManager,
        IChatObjectCategoryManager chatObjectCategoryManager,
        ISessionPermissionChecker sessionPermissionChecker)
    {
        RoomManager = roomManager;
        ChatObjectCategoryManager = chatObjectCategoryManager;
        SessionPermissionChecker = sessionPermissionChecker;
    }


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

    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateByAllUsersAsync(string name)
    {
        var entity = await RoomManager.CreateByAllUsersAsync(name);

        return await MapToChatObjectDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<List<SessionUnitSenderInfo>> InviteAsync(InviteInput input)
    {
        await CheckPolicyAsync(InvitePolicyName);

        var list = await RoomManager.InviteAsync(input, autoSendMessage: true);

        return ObjectMapper.Map<List<SessionUnit>, List<SessionUnitSenderInfo>>(list);
    }

    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetSameAsync(SameGetListInput input)
    {
        var query = await SessionUnitManager.GetSameSessionQeuryableAsync(input.SourceChatObjectId, input.TargetChatObjectId, new List<ChatObjectTypeEnums>() { ChatObjectTypeEnums.Room });

        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input, null);
    }

    [HttpGet]
    public Task<int> GetSameCountAsync(long sourceChatObjectId, long targetChatObjectId)
    {
        return SessionUnitManager.GetSameSessionCountAsync(sourceChatObjectId, targetChatObjectId, new List<ChatObjectTypeEnums>() { ChatObjectTypeEnums.Room });
    }

    [HttpPost]
    public async Task<ChatObjectDto> UpdateNameAsync(Guid sessionUnitId, string name)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        //await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName, sessionUnit);

        var entity = await RoomManager.UpdateNameAsync(sessionUnit, name);

        return await MapToChatObjectDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<ChatObjectDto> UpdatePortraitAsync(Guid sessionUnitId, string thubmnail, string portrait)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdatePortrait, sessionUnit);

        var entity = await RoomManager.UpdatePortraitAsync(sessionUnit, thubmnail, portrait);

        return await MapToChatObjectDtoAsync(entity);
    }

    [HttpPost]
    public virtual Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId)
    {
        return RoomManager.TransferCreatorAsync(sessionUnitId, targetSessionUnitId);
    }

    [HttpPost]
    public virtual Task DissolveAsync(Guid sessionUnitId)
    {
        throw new NotImplementedException();
    }
}
