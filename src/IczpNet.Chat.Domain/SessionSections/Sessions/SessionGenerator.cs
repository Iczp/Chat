using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using System.Security.Cryptography;
using IczpNet.AbpCommons;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionGenerator : DomainService, ISessionGenerator
    {
        protected IMessageRepository MessageRepository => LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
        protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        protected IChannelResolver ChannelResolver => LazyServiceProvider.LazyGetRequiredService<IChannelResolver>();
        protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
        protected IChatObjectManager ChatObjectManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();

        public SessionGenerator() { }

        protected virtual Task<Guid> MakeSesssionIdAsync(string input)
        {
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));

            string hashString = string.Join(string.Empty, hashBytes.Select(x => x.ToString("X2")));

            return Task.FromResult(new Guid(hashString));
        }

        protected virtual bool IsObjectType(ChatObjectTypeEnums input, params ChatObjectTypeEnums[] objectTypes)
        {
            return objectTypes.Contains(input);
        }

        private async void ResolveShopWaiterId(IChatObject sender, IChatObject receiver, Func<long, Task> matchAction)
        {
            if (IsObjectType(receiver.ObjectType.Value, ChatObjectTypeEnums.ShopWaiter))
            {
                var list = new List<ChatObjectTypeEnums>() {
                    ChatObjectTypeEnums.Personal,
                    ChatObjectTypeEnums.Anonymous,
                    ChatObjectTypeEnums.Customer,
                };
                if (receiver.ParentId.HasValue && list.Contains(sender.ObjectType.Value))
                {
                    await matchAction?.Invoke(receiver.ParentId.Value);
                }
            }
        }
        protected virtual async Task<string> MakeSesssionKeyAsync(IChatObject sender, IChatObject receiver)
        {
            await Task.CompletedTask;

            var senderId = sender.Id;

            var receiverId = receiver.Id;

            if (IsObjectType(receiver.ObjectType.Value, ChatObjectTypeEnums.Room, ChatObjectTypeEnums.Square))
            {
                return receiverId.ToString();
            }

            if (IsObjectType(sender.ObjectType.Value, ChatObjectTypeEnums.Room, ChatObjectTypeEnums.Square))
            {
                return senderId.ToString();
            }

            ResolveShopWaiterId(sender, receiver, async (v) =>
            {
                receiverId = v;
                await Task.CompletedTask;
            });

            ResolveShopWaiterId(receiver, sender, async (v) =>
            {
                senderId = v;
                await Task.CompletedTask;
            });

            var arr = new[] { senderId, receiverId };

            Array.Sort(arr);

            return string.Join(":", arr);
        }

        public virtual Task<Session> MakeAsync(IChatObject room)
        {
            return MakeAsync(room, room);
        }

        public virtual async Task<Session> MakeAsync(IChatObject sender, IChatObject receiver)
        {
            var sessionKey = await MakeSesssionKeyAsync(sender, receiver);

            var sessionId = await MakeSesssionIdAsync(sessionKey);

            var session = await SessionRepository.FindAsync(x => x.Id.Equals(sessionId));

            if (session != null)
            {
                return session;
            }

            var channel = await ChannelResolver.GetAsync(sender, receiver);

            session = new Session(sessionId, sessionKey, channel);

            ResolveShopWaiterId(sender, receiver, async (shopKeeperId) =>
            {
                //add sender
                var shopKeeper = await ChatObjectManager.GetItemByCacheAsync(shopKeeperId);

                session.AddSessionUnit(new SessionUnit(
                        id: GuidGenerator.Create(),
                        session: session,
                        ownerId: sender.Id,
                        destinationId: shopKeeper.Id,
                        destinationObjectType: shopKeeper.ObjectType));

                //Cache GetChildsByCacheAsync
                var shopWaiterList = await ChatObjectManager.GetChildsAsync(shopKeeperId);

                foreach (var shopWaiter in shopWaiterList)
                {
                    session.AddSessionUnit(new SessionUnit(
                        id: GuidGenerator.Create(),
                        session: session,
                        ownerId: shopWaiter.Id,
                        destinationId: sender.Id,
                        destinationObjectType: sender.ObjectType));
                }

                

                //add or update sessionUnit
                await Task.CompletedTask;
            });

            //if (sender.ObjectType == ChatObjectTypeEnums.Official)
            //{
            //    Assert.If(receiver.ObjectType != ChatObjectTypeEnums.Personal && sender.Id != receiver.Id, "非法");
            //}

            //if (channel == Channels.PrivateChannel)
            //{
            //    session.AddSessionUnit(new SessionUnit(GuidGenerator.Create(), session, sender.Id, receiver.Id, receiver.ObjectType));

            //    if (sender.Id != receiver.Id)
            //    {
            //        session.AddSessionUnit(new SessionUnit(GuidGenerator.Create(), session, receiver.Id, sender.Id, sender.ObjectType));
            //    }
            //}
            return await SessionRepository.InsertAsync(session, autoSave: true);
        }

        [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
        public virtual async Task<List<Session>> GenerateSessionByMessageAsync()
        {
            var list = (await MessageRepository.GetQueryableAsync())
                .Where(x => x.SessionId == null)
                .GroupBy(x => x.SessionKey, (SessionValue, Items) => new
                {
                    SessionValue,
                    Items = Items.ToList()
                })
                .ToList();

            var sessionList = new List<Session>();

            foreach (var item in list)
            {
                var unitList = new List<SessionUnit>();

                var session = new Session(GuidGenerator.Create(), item.SessionValue, Channels.PrivateChannel);

                foreach (var message in item.Items)
                {
                    if (!unitList.Any(x => x.OwnerId == message.SenderId.Value && x.DestinationId == message.ReceiverId.Value))
                    {
                        unitList.Add(new SessionUnit(GuidGenerator.Create(), session, message.Sender.Id, message.Receiver.Id, message.Receiver.ObjectType));
                    }
                    if (!unitList.Any(x => x.OwnerId == message.ReceiverId.Value && x.DestinationId == message.SenderId.Value))
                    {
                        unitList.Add(new SessionUnit(GuidGenerator.Create(), session, message.Receiver.Id, message.Sender.Id, message.Sender.ObjectType));
                    }
                }
                session.SetMessageList(item.Items);

                session.SetUnitList(unitList);

                sessionList.Add(session);
            }
            await SessionRepository.InsertManyAsync(sessionList);

            return sessionList;
        }

        public async Task<Session> UpdateAsync(Session session)
        {
            if (session.LastMessageId.HasValue)
            {
                await SessionUnitManager.BatchUpdateAsync(session.Id, session.LastMessageId.Value);
            }
            return await SessionRepository.UpdateAsync(session, true);

        }


    }
}
