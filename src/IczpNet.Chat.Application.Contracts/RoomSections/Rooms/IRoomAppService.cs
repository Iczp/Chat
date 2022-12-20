using IczpNet.Chat.RoomSections.Rooms.Dtos;
using System;
using System.Threading.Tasks;
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

    Task<int> JoinRoomAsync(Guid id, RoomJoinInput input);
}
