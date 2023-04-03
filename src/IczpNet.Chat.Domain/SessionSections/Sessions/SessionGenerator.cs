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

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionGenerator : DomainService, ISessionGenerator
    {
        protected IMessageRepository MessageRepository => LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
        protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        protected IChannelResolver ChannelResolver => LazyServiceProvider.LazyGetRequiredService<IChannelResolver>();
        protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

        public SessionGenerator() { }

        protected virtual Task<Guid> MakeSesssionIdAsync(string input)
        {
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));

            string hashString = string.Join(string.Empty, hashBytes.Select(x => x.ToString("X2")));

            return Task.FromResult(new Guid(hashString));
        }

        protected virtual bool IsObjectType(IChatObject chatObject, ChatObjectTypeEnums chatObjectTypeEnums)
        {
            return chatObject.ObjectType.Equals(chatObjectTypeEnums) || chatObject.ChatObjectTypeId == chatObjectTypeEnums.ToString();
        }

        protected virtual async Task<string> MakeSesssionKeyAsync(IChatObject sender, IChatObject receiver)
        {
            await Task.CompletedTask;

            if (IsObjectType(sender, ChatObjectTypeEnums.Room))
            {
                return sender.Id.ToString();
            }
            if (IsObjectType(receiver, ChatObjectTypeEnums.Room))
            {
                return receiver.Id.ToString();
            }
            var arr = new[] { sender.Id, receiver.Id };

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
