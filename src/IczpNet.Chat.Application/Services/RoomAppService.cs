using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IczpNet.Chat.Services;

public class RoomAppService : ChatAppService, IRoomAppService
{
    protected IRoomManager RoomManager { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }

    public RoomAppService(IRoomManager roomManager, IChatObjectCategoryManager chatObjectCategoryManager)
    {
        RoomManager = roomManager;
        ChatObjectCategoryManager = chatObjectCategoryManager;
    }


    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateAsync(RoomCreateInput input)
    {
        var room = await RoomManager.CreateAsync(input.Name, input.ChatObjectIdList, input.OwnerId);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(room);
    }

    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateByAllUsersAsync(string name)
    {
        var room = await RoomManager.CreateByAllUsersAsync(name);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(room);
    }
}
