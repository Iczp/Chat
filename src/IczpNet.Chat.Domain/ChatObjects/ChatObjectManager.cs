using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectManager : DomainService, IChatObjectManager
    {

        protected IReadOnlyRepository<ChatObject, Guid> ChatObjectReadOnlyRepository { get; }

        public ChatObjectManager(IReadOnlyRepository<ChatObject, Guid> chatObjectReadOnlyRepository)
        {
            ChatObjectReadOnlyRepository = chatObjectReadOnlyRepository;
        }

        public async Task<List<ChatObject>> GetListByUserId(Guid userId)
        {
            return await ChatObjectReadOnlyRepository.GetListAsync(x => x.AppUserId == userId);
        }

        public Task<ChatObject> GetAsync(Guid chatObjectId)
        {
            return ChatObjectReadOnlyRepository.GetAsync(chatObjectId);
        }

        public async Task<List<ChatObject>> GetManyAsync(List<Guid> chatObjectIdList)
        {
            var list = new List<ChatObject>();

            foreach (var chatObjectId in chatObjectIdList)
            {
                list.Add(await GetAsync(chatObjectId));
            }
            return list;
        }

        public Task<bool> IsAllowJoinRoomAsync(ChatObjectTypes? objectType)
        {
            return Task.FromResult(ChatConsts.AllowJoinRoomObjectTypes.Any(x => x.Equals(objectType)));
        }

        public virtual async Task<List<Guid>> GetIdListByNameAsync(List<string> nameList)
        {
            var query = (await ChatObjectReadOnlyRepository.GetQueryableAsync())
                .Where(x => nameList.Contains(x.Name))
                .Select(x => x.Id)
                ;
            return await AsyncExecuter.ToListAsync(query);
        }

        public async Task<List<ChatObject>> GetAllListAsync(ChatObjectTypes objectType)
        {
            var query = (await ChatObjectReadOnlyRepository.GetQueryableAsync())
                .Where(x => x.ObjectType == objectType)
                ;
            return await AsyncExecuter.ToListAsync(query);
        }
    }
}
