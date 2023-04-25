using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public interface IRoomManager
    {
        Task<bool> IsAllowJoinRoomAsync(ChatObjectTypeEnums objectType);

        Task<bool> IsAllowCreateRoomAsync(ChatObjectTypeEnums objectType);

        Task<ChatObject> CreateAsync(string name, List<long> memberIdList, long? ownerId);

        Task<ChatObject> CreateWithManyAsync(string name, List<long> memberIdList, long? ownerId);

        Task<ChatObject> CreateByAllUsersAsync(string name);

        Task<ChatObject> CreateByAllUsersWithManyAsync(string name);

        Task<List<SessionUnit>> InviteAsync(InviteInput input, bool autoSendMessage = true);

        //Task<ChatObject> UpdateAsync(ChatObject room);

        Task<int> GetMemberCountAsync(ChatObject room);

        Task<int> JoinRoomAsync(ChatObject room, List<ChatObject> members, ChatObject inviter, JoinWays joinWay);

        Task<bool> IsInRoomAsync(ChatObject room, ChatObject member);

        Task<bool> IsInRoomAsync(Guid sessionId, IEnumerable<long> memberIdList);

        Task<bool> IsInRoomAsync(Guid sessionId, long memberId);

        Task<IQueryable<SessionUnit>> GetSameGroupAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<ChatObject> UpdateNameAsync(SessionUnit sessionUnit, string name);

        Task<ChatObject> UpdatePortraitAsync(SessionUnit sessionUnit, string portrait);

        Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId, bool isSendMessageToRoom = true);
    }
}
