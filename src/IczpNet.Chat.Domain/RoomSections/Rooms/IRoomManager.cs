using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public interface IRoomManager
    {

        Task<bool> IsAllowJoinRoomAsync(ChatObjectTypes? objectType);

        Task<bool> IsAllowCreateRoomAsync(ChatObjectTypes? objectType);

        Task<Room> CreateRoomAsync(Room room, List<ChatObject> members);

        Task<Room> CreateRoomAsync(Room room, List<Guid> chatObjectIdList);

        Task<int> JoinRoomAsync(Room room, List<ChatObject> members, ChatObject inviter, JoinWays joinWay);

        Task<bool> IsInRoomAsync(Room room, ChatObject member);

    }
}
