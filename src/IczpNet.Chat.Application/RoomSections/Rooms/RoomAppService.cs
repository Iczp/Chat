using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RoomSections.Rooms;

/// <summary>
/// 群
/// </summary>
public class RoomAppService : ChatAppService, IRoomAppService
{
    public virtual string InvitePolicyName { get; set; }
    public virtual string CreateRoomPolicyName { get; set; }
    protected IRoomManager RoomManager { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
    protected ISessionPermissionChecker SessionPermissionChecker { get; }

    public RoomAppService(IRoomManager roomManager,
        IChatObjectCategoryManager chatObjectCategoryManager,
        ISessionPermissionChecker sessionPermissionChecker)
    {
        RoomManager = roomManager;
        ChatObjectCategoryManager = chatObjectCategoryManager;
        SessionPermissionChecker = sessionPermissionChecker;
    }

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
        var query = await SessionUnitManager.GetSameSessionQeuryableAsync(input.SourceChatObjectId, input.TargetChatObjectId, new List<ChatObjectTypeEnums>() { ChatObjectTypeEnums.Room });

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
    /// 转让群主
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="targetSessionUnitId">目标会话单元Id(新群主Id)</param>
    /// <returns></returns>
    [HttpPost]
    public virtual Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId)
    {
        return RoomManager.TransferCreatorAsync(sessionUnitId, targetSessionUnitId);
    }

    /// <summary>
    /// 解散群
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public virtual Task DissolveAsync(Guid sessionUnitId)
    {
        throw new NotImplementedException();
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
