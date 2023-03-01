
using AutoMapper.Execution;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectManager : TreeManager<ChatObject, Guid, ChatObjectInfo>, IChatObjectManager
    {
        protected IChatObjectTypeManager ChatObjectTypeManager { get; }
        protected IMessageSender MessageSender => LazyServiceProvider.LazyGetRequiredService<IMessageSender>();
        public ChatObjectManager(
            IRepository<ChatObject, Guid> repository,
            IChatObjectTypeManager chatObjectTypeManager) : base(repository)
        {
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

        public virtual Task<bool> IsAllowJoinRoomAsync(ChatObjectTypeEnums? objectType)
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

        public virtual async Task<List<ChatObject>> GetAllListAsync(ChatObjectTypeEnums objectType)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.ObjectType == objectType)
                ;
            return await AsyncExecuter.ToListAsync(query);
        }

        public virtual async Task<ChatObject> CreateRoomAsync(string name, List<Guid> memberList, Guid? ownerId)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

            var members = await GetManyByCacheAsync(memberList);

            var room = new ChatObject(GuidGenerator.Create(), name, chatObjectType, null);

            var session = new Session(room.Id, room.Id.ToString(), Channels.RoomChannel);

            session.SetOwner(room);

            foreach (var member in members)
            {
                session.AddSessionUnit(new SessionUnit(GuidGenerator.Create(), session, member.Id, room.Id, room.ObjectType));
            }

            room.OwnerSessionList.Add(session);

            await base.CreateAsync(room, isUnique: false);

            var roomOwner = ownerId.HasValue ? await GetItemByCacheAsync(ownerId.Value) : null;

            await MessageSender.SendCmdMessageAsync(new MessageInput<CmdContentInfo>()
            {
                SenderId = room.Id,
                ReceiverId = room.Id,
                Content = new CmdContentInfo()
                {
                    Text = $"{roomOwner?.Name}创建群,{members.Take(3).Select(x => x.Name).JoinAsString("、")}等 {members.Count} 人加入群聊。",
                }
            });

            return room;
        }

        public virtual async Task<ChatObject> CreateRoomByAllUsersAsync(string name)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.ObjectType == ChatObjectTypeEnums.Personal)
                .Select(x => x.Id)
                ;
            var idList = await AsyncExecuter.ToListAsync(query);

            return await CreateRoomAsync(name, idList, null);
        }
    }
}
