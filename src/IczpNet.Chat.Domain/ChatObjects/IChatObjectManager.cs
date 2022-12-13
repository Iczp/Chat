using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectManager
    {
        Task<List<ChatObject>> GetListByUserId(Guid userId);

        Task<ChatObject> GetAsync(Guid chatObjectId);

        Task<bool> IsAllowJoinRoomMemnerAsync(ChatObjectTypes? objectType);

        Task<List<Guid>> GetIdListByNameAsync(List<string> nameList); //, List<ChatObjectTypes> objectTypes
    }
}
