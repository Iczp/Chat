using IczpNet.AbpTrees;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectManager : ITreeManager<ChatObject, Guid, ChatObjectInfo>
    {
        Task<List<ChatObject>> GetAllListAsync(ChatObjectTypeEnums objectType);

        Task<List<ChatObject>> GetListByUserId(Guid userId);

        Task<List<Guid>> GetIdListByUserId(Guid userId);

        //Task<ChatObject> GetAsync(Guid chatObjectId);

        //Task<ChatObjectInfo> GetItemByCacheAsync(Guid chatObjectId);

        //Task<List<ChatObjectInfo>> GetManyByCacheAsync(List<Guid> chatObjectIdList);

        //Task<List<ChatObject>> GetManyAsync(List<Guid> chatObjectIdList);

        Task<bool> IsAllowJoinRoomAsync(ChatObjectTypeEnums? objectType);

        Task<List<Guid>> GetIdListByNameAsync(List<string> nameList); //, List<ChatObjectTypes> objectTypes

        Task<ChatObject> CreateRoomAsync(string name, List<Guid> memberList, Guid? ownerId);

    }
}
