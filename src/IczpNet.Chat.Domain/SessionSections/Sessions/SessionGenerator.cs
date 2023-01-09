using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionGenerator : DomainService, ISessionGenerator
    {
        protected IChatObjectManager ChatObjectManager { get; }
        protected IRepository<Message, Guid> MessageRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected IRepository<ReadedRecorder, Guid> ReadedRecorderRepository { get; }
        protected ISessionRecorder SessionRecorder { get; }
        protected IChannelResolver ChannelResolver { get; }


        public SessionGenerator(
            IChatObjectManager chatObjectManager,
            IRepository<Message, Guid> messageRepository,
            IRepository<Session, Guid> sessionRepository,
            ISessionRecorder sessionRecorder,
            IRepository<ReadedRecorder, Guid> readedRecorderRepository,
            IChannelResolver channelResolver)
        {
            ChatObjectManager = chatObjectManager;
            MessageRepository = messageRepository;
            SessionRepository = sessionRepository;
            SessionRecorder = sessionRecorder;
            ReadedRecorderRepository = readedRecorderRepository;
            ChannelResolver = channelResolver;
        }

        protected virtual string MakeSesssionKey(ChatObject sender, ChatObject receiver)
        {
            if (sender.ObjectType.Equals(ChatObjectTypes.Room))
            {
                return sender.Id.ToString();
            }
            if (receiver.ObjectType.Equals(ChatObjectTypes.Room))
            {
                return receiver.Id.ToString();
            }
            var arr = new[] { sender.Id, receiver.Id };

            Array.Sort(arr);

            return string.Join(":", arr);
        }

        public async Task<Session> MakeAsync(ChatObject sender, ChatObject receiver)
        {
            var sessionKey = MakeSesssionKey(sender, receiver);

            var session = await SessionRepository.FindAsync(x => x.SessionKey.Equals(sessionKey));

            if (session != null)
            {
                return session;
            }

            var channel = await ChannelResolver.GetAsync(sender, receiver);

            session = new Session(GuidGenerator.Create(), sessionKey, channel);

            if (sender.ObjectType == ChatObjectTypes.Official)
            {
                Assert.If(receiver.ObjectType != ChatObjectTypes.Personal && sender.Id != receiver.Id, "非法");
            }

            if (channel == Channels.PrivateChannel)
            {
                session.AddSessionUnit(new SessionUnit(GuidGenerator.Create(), session, sender, receiver));

                if (sender.Id != receiver.Id)
                {
                    session.AddSessionUnit(new SessionUnit(GuidGenerator.Create(), session, receiver, sender));
                }
            }
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
                        unitList.Add(new SessionUnit(GuidGenerator.Create(), session, message.Sender, message.Receiver));
                    }
                    if (!unitList.Any(x => x.OwnerId == message.ReceiverId.Value && x.DestinationId == message.SenderId.Value))
                    {
                        unitList.Add(new SessionUnit(GuidGenerator.Create(), session, message.Receiver, message.Sender));
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
            return await SessionRepository.UpdateAsync(session, true);
        }
    }
}
