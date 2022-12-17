using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections
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

            var channel = await ChannelResolver.GetAsync(sender, receiver);

            var session = await SessionRepository.FindAsync(x => x.SessionKey.Equals(sessionKey));

            if (session != null)
            {
                return session;
            }

            session = new Session(GuidGenerator.Create(), sessionKey, channel);

            if (channel == Channels.PrivateChannel )
            {
                session.AddSessionUnit(new SessionUnit(session.Id, sender.Id, receiver.Id));
                if ( sender.Id != receiver.Id)
                {
                    session.AddSessionUnit(new SessionUnit(session.Id, receiver.Id, sender.Id));
                }
            }
            return await SessionRepository.InsertAsync(session, autoSave: true);
        }

        [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
        public async Task<List<Session>> GenerateAsync(Guid ownerId, long? startMessageAutoId = null)
        {

            //return await CreateSessionAsync();
            var currentTick = Clock.Now.Ticks;
            var owner = await ChatObjectManager.GetAsync(ownerId);
            //用户所在的群(包含已经删除的)
            //owner.InRoomMemberList
            var minAutoId = startMessageAutoId ?? await SessionRecorder.GetAsync(owner);

            var messageQuery = await MessageRepository.GetQueryableAsync();

            // 个人消息
            var personalMessage = messageQuery
                .Where(q => q.Channel == Channels.PrivateChannel)
                .Where(q => q.SenderId == ownerId || q.ReceiverId == ownerId);


            // 已读
            var readedRecorderList = await SessionRecorder.GetReadedsAsync(ownerId);

            var predicate = PredicateBuilder.New<Message>();
            predicate = predicate.And(x => x.SenderId != ownerId);


            var readedQuery = (await ReadedRecorderRepository.GetQueryableAsync()).Where(x => x.OwnerId == ownerId);

            var list = messageQuery
                .GroupBy(x => x.SessionKey, (SessionId, g) => new
                {
                    SessionId,
                    Message = g.Where(x => x.AutoId == g.Max(c => c.AutoId)),
                    AutoId = g.Max(c => c.AutoId),
                    //未读消息数量
                    UnreadCount = g.Where(m => m.SenderId != ownerId).Count(),
                    // @我（包含 @所有人）
                    ReminderCount = g.Where(m => m.SenderId != ownerId)
                                     .Where(x => x.KeyName == MessageKeyNames.Remind)
                                     .Where(x => x.KeyValue == MessageKeyNames.RemindEveryone || x.KeyValue.IndexOf(ownerId.ToString()) != -1)
                                     .Count()
                })
                //.OrderByDescending(t => t.AutoId)
                .ToList()
                ;

            var sessionList = list.Select(x => new Session(GuidGenerator.Create(), x.SessionId, Channels.PrivateChannel)).ToList();

            await SessionRepository.InsertManyAsync(sessionList);

            return sessionList;
        }


        public virtual async Task<List<Session>> CreateSessionAsync()
        {
            var list = (await MessageRepository.GetQueryableAsync())
                .GroupBy(x => x.SessionKey, (SessionValue, Items) => new
                {
                    SessionValue,
                    Items = Items.ToList()
                })
                .ToList();
            ;
            var sessionList = new List<Session>();
            foreach (var item in list)
            {
                var memberList = new List<SessionUnit>();
                var sessionId = GuidGenerator.Create();
                foreach (var message in item.Items)
                {
                    if (!memberList.Any(x => x.OwnerId == message.SenderId.Value && x.DestinationId == message.ReceiverId.Value))
                    {
                        memberList.Add(new SessionUnit(sessionId, message.SenderId.Value, message.ReceiverId.Value));
                    }
                    if (!memberList.Any(x => x.OwnerId == message.ReceiverId.Value && x.DestinationId == message.SenderId.Value))
                    {
                        memberList.Add(new SessionUnit(sessionId, message.ReceiverId.Value, message.SenderId.Value));
                    }
                }
                var session = new Session(sessionId, item.SessionValue, Channels.PrivateChannel)
                {
                    MessageList = item.Items,
                    UnitList = memberList
                };
                sessionList.Add(session);
            }
            await SessionRepository.InsertManyAsync(sessionList);

            return sessionList;
        }
    }
}
