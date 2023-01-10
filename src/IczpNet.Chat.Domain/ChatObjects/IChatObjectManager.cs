using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectManager
    {
        Task<List<ChatObject>> GetAllListAsync(ChatObjectTypes objectType);

        Task<List<ChatObject>> GetListByUserId(Guid userId);

        Task<ChatObject> GetAsync(Guid chatObjectId);

        Task<List<ChatObject>> GetManyAsync(List<Guid> chatObjectIdList);

        Task<bool> IsAllowJoinRoomAsync(ChatObjectTypes? objectType);

        Task<List<Guid>> GetIdListByNameAsync(List<string> nameList); //, List<ChatObjectTypes> objectTypes
        
    }
}
