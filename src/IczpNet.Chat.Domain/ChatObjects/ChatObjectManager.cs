
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectManager : TreeManager<ChatObject, Guid, ChatObjectInfo>, IChatObjectManager
    {
        protected IChatObjectTypeManager ChatObjectTypeManager { get; }
        protected IDistributedCache<ChatObjectInfo, Guid> ChatObjectCache { get; }

        public ChatObjectManager(
            IRepository<ChatObject, Guid> repository,
            IDistributedCache<ChatObjectInfo, Guid> chatObjectCache,
              IChatObjectTypeManager chatObjectTypeManager) : base(repository)
        {
            ChatObjectCache = chatObjectCache;
            ChatObjectTypeManager = chatObjectTypeManager;
        }

        public async Task<List<ChatObject>> GetListByUserId(Guid userId)
        {
            return await Repository.GetListAsync(x => x.AppUserId == userId);
        }

        public async Task<List<Guid>> GetIdListByUserId(Guid userId)
        {
            return (await Repository.GetQueryableAsync()).Where(x => x.AppUserId == userId).Select(x => x.Id).ToList();
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
            var query = (await Repository.GetQueryableAsync())
                .Where(x => nameList.Contains(x.Name))
                .Select(x => x.Id)
                ;
            return await AsyncExecuter.ToListAsync(query);
        }

        public async Task<List<ChatObject>> GetAllListAsync(ChatObjectTypeEnums objectType)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.ObjectType == objectType)
                ;
            return await AsyncExecuter.ToListAsync(query);
        }

        public async Task<ChatObject> CreateRoomAsync(string name, List<Guid> memberList)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

            
            var room = new ChatObject(GuidGenerator.Create(), name, chatObjectType, null);

            var session = new Session(room.Id, room.Id.ToString(), Channels.RoomChannel);

            session.SetOwner(room);

            foreach (var chatObjectId in memberList)
            {
                session.AddSessionUnit(new SessionUnit(GuidGenerator.Create(), session, chatObjectId, room.Id, room.ObjectType));
            }
            room.OwnerSessionList.Add(session);

            await base.CreateAsync(room);

            return room;
        }
    }
}
