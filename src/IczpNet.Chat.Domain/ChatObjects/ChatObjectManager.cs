
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectManager : DomainService, IChatObjectManager
    {

        protected IReadOnlyRepository<ChatObject, Guid> ChatObjectReadOnlyRepository { get; }
        protected IDistributedCache<ChatObjectInfo, Guid> ChatObjectCache { get; }

        protected IObjectMapper ObjectMapper { get; }

        public ChatObjectManager(
            IReadOnlyRepository<ChatObject, Guid> chatObjectReadOnlyRepository,
            IDistributedCache<ChatObjectInfo, Guid> chatObjectCache,
            IObjectMapper objectMapper)
        {
            ChatObjectReadOnlyRepository = chatObjectReadOnlyRepository;
            ChatObjectCache = chatObjectCache;
            ObjectMapper = objectMapper;
        }

        public async Task<List<ChatObject>> GetListByUserId(Guid userId)
        {
            return await ChatObjectReadOnlyRepository.GetListAsync(x => x.AppUserId == userId);
        }

        public async Task<List<Guid>> GetIdListByUserId(Guid userId)
        {
            return (await ChatObjectReadOnlyRepository.GetQueryableAsync()).Where(x => x.AppUserId == userId).Select(x => x.Id).ToList();
        }

        public Task<ChatObject> GetAsync(Guid chatObjectId)
        {
            return ChatObjectReadOnlyRepository.GetAsync(chatObjectId);
        }

        public Task<ChatObjectInfo> GetItemByCacheAsync(Guid chatObjectId)
        {
            return ChatObjectCache.GetOrAddAsync(chatObjectId, async () =>
            {
                var entity = await GetAsync(chatObjectId);
                return ObjectMapper.Map<ChatObject, ChatObjectInfo>(entity);
            });
        }

        public async Task<List<ChatObjectInfo>> GetManyByCacheAsync(List<Guid> chatObjectIdList)
        {
            var list = new List<ChatObjectInfo>();

            foreach (var chatObjectId in chatObjectIdList)
            {
                list.Add(await GetItemByCacheAsync(chatObjectId));
            }
            return list;
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

        public Task<bool> IsAllowJoinRoomAsync(ChatObjectTypeEnums? objectType)
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

        public async Task<List<ChatObject>> GetAllListAsync(ChatObjectTypeEnums objectType)
        {
            var query = (await ChatObjectReadOnlyRepository.GetQueryableAsync())
                .Where(x => x.ObjectType == objectType)
                ;
            return await AsyncExecuter.ToListAsync(query);
        }
    }
}
