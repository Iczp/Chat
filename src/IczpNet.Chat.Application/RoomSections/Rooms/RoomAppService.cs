using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomAppService : ChatAppService, IRoomAppService
{
    public virtual string InvitePolicyName { get; set; }
    public virtual string CreateRoomPolicyName { get; set; }
    protected IRoomManager RoomManager { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
    protected ISessionPermissionChecker SessionPermissionChecker { get; }

    public RoomAppService(IRoomManager roomManager,
        IChatObjectCategoryManager chatObjectCategoryManager,
        ISessionUnitManager sessionUnitManager,
        ISessionPermissionChecker sessionPermissionChecker)
    {
        RoomManager = roomManager;
        ChatObjectCategoryManager = chatObjectCategoryManager;
        SessionUnitManager = sessionUnitManager;
        SessionPermissionChecker = sessionPermissionChecker;
    }


    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateAsync(RoomCreateInput input)
    {
        await CheckPolicyAsync(CreateRoomPolicyName);

        var room = await RoomManager.CreateAsync(input.Name, input.ChatObjectIdList, input.OwnerId);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(room);
    }

    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateByAllUsersAsync(string name)
    {
        var room = await RoomManager.CreateByAllUsersAsync(name);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(room);
    }

    [HttpPost]
    public virtual async Task<List<SessionUnitSenderInfo>> InviteAsync(InviteInput input)
    {
        await CheckPolicyAsync(InvitePolicyName);

        var list = await RoomManager.InviteAsync(input, autoSendMessage: true);

        return ObjectMapper.Map<List<SessionUnit>, List<SessionUnitSenderInfo>>(list);
    }

    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetSameGroupAsync(long sourceChatObjectId, long targetChatObjectId, int maxResultCount = 10,
        int skipCount = 0, string sorting = null)
    {
        var query = await RoomManager.GetSameGroupAsync(sourceChatObjectId, targetChatObjectId, new List<ChatObjectTypeEnums>() { ChatObjectTypeEnums.Room });

        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, maxResultCount, skipCount, sorting, null);
    }

    [HttpGet]
    public async Task<int> GetSameGroupCountAsync(long sourceChatObjectId, long targetChatObjectId)
    {
        var query = await RoomManager.GetSameGroupAsync(sourceChatObjectId, targetChatObjectId, new List<ChatObjectTypeEnums>() { ChatObjectTypeEnums.Room });

        return await AsyncExecuter.CountAsync(query);
    }
}
