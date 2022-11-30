using IczpNet.Chat.RoomSections.Rooms.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.RoomSections.Rooms;

public interface IRoomAppService :
    ICrudAppService<
        RoomDetailDto,
        RoomDto,
        Guid,
        RoomGetListInput,
        RoomCreateInput,
        RoomUpdateInput>
{
}
