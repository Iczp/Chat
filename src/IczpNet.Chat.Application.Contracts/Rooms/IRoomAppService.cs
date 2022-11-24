using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Rooms.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Rooms
{
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
}
