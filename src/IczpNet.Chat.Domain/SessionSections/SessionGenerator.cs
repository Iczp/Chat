using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.Sessions;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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


        public SessionGenerator(
            IChatObjectManager chatObjectManager,
            IRepository<Message, Guid> messageRepository,
            IRepository<Session, Guid> sessionRepository,
            ISessionRecorder sessionRecorder,
            IRepository<ReadedRecorder, Guid> readedRecorderRepository)
        {
            ChatObjectManager = chatObjectManager;
            MessageRepository = messageRepository;
            SessionRepository = sessionRepository;
            SessionRecorder = sessionRecorder;
            ReadedRecorderRepository = readedRecorderRepository;
        }

        protected virtual string MakeSesssionId(MessageChannels messageChannel, ChatObject sender, ChatObject receiver)
        {
            switch (messageChannel)
            {
                case MessageChannels.RoomChannel:
                case MessageChannels.SubscriptionChannel:
                case MessageChannels.ServiceChannel:
                case MessageChannels.SquareChannel:
                    return receiver.Id.ToString();
                case MessageChannels.PersonalToPersonal:
                case MessageChannels.RobotChannel:
                    var arr = new[] { sender.Id, receiver.Id };
                    Array.Sort(arr);
                    return string.Join(":", arr);
                case MessageChannels.ElectronicCommerceChannel:
                default:
                    return null;
            }
        }

        public Task<string> MakeSesssionIdAsync(MessageChannels messageChannel, ChatObject sender, ChatObject receiver)
        {
            return Task.FromResult(MakeSesssionId(messageChannel, sender, receiver));
        }

        [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
        public async Task<List<Session>> GenerateAsync(Guid ownerId, long? startMessageAutoId = null)
        {
            var currentTick = Clock.Now.Ticks;
            var owner = await ChatObjectManager.GetAsync(ownerId);
            //用户所在的群(包含已经删除的)
            //owner.InRoomMemberList
            var minAutoId = startMessageAutoId ?? await SessionRecorder.GetAsync(owner);

            var messageQuery = await MessageRepository.GetQueryableAsync();

            // 个人消息
            var personalMessage = messageQuery
                .Where(q => q.MessageChannel == MessageChannels.PersonalToPersonal)
                .Where(q => q.SenderId == ownerId || q.ReceiverId == ownerId);


            // 已读
            var readedRecorderList = await SessionRecorder.GetReadedsAsync(ownerId);

            var predicate = PredicateBuilder.New<Message>();
            predicate = predicate.And(x => x.SenderId != ownerId);


            var readedQuery = (await ReadedRecorderRepository.GetQueryableAsync()).Where(x => x.OwnerId == ownerId);

            var list = messageQuery
                .GroupBy(x => x.SessionId, (SessionId, g) => new
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

            var sessionList = list.Select(x => new Session(GuidGenerator.Create(), ownerId)
            {
                SessionId = x.SessionId,
                //DestinationId = 
                MessageAutoId = x.AutoId,
                Badge = x.UnreadCount,
                Description = $"@我:{x.ReminderCount}"
            }).ToList();

            await SessionRepository.InsertManyAsync(sessionList);

            return sessionList;
        }
    }
}
