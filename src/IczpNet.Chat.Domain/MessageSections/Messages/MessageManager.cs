using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Etos;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageManager : DomainService, IMessageManager
    {
        protected IObjectMapper ObjectMapper { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        protected IRepository<Message, Guid> Repository { get; }
        protected ISessionGenerator SessionGenerator { get; }
        protected IMessageValidator MessageValidator { get; }
        protected IChatObjectResolver ChatObjectResolver { get; }
        protected IContentResolver ContentResolver { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        
        protected ChatOption Config { get; }
        protected IChatPusher ChatPusher { get; }

        public MessageManager(
            IRepository<Message, Guid> repository,
            IChatObjectResolver messageChatObjectResolver,
            IChatObjectManager chatObjectManager,
            IContentResolver contentResolver,
            IObjectMapper objectMapper,
            IMessageValidator messageValidator,
            ISessionGenerator sessionIdGenerator,
            IOptions<ChatOption> options,
            IChatPusher chatPusher,
            ISessionUnitManager sessionUnitManager)
        {
            Repository = repository;
            ChatObjectResolver = messageChatObjectResolver;
            ChatObjectManager = chatObjectManager;
            ContentResolver = contentResolver;
            ObjectMapper = objectMapper;
            MessageValidator = messageValidator;
            SessionGenerator = sessionIdGenerator;
            Config = options.Value;
            ChatPusher = chatPusher;
            SessionUnitManager = sessionUnitManager;
        }

        public virtual async Task<Message> CreateMessageAsync(ChatObjectInfo sender, ChatObjectInfo receiver, Func<Message, Task<IMessageContentEntity>> func)
        {
            var session = await SessionGenerator.MakeAsync(sender, receiver);

            var entity = new Message(GuidGenerator.Create(), sender, receiver, session);

            if (func != null)
            {
                var messageContent = await func(entity);
                entity.SetMessageContent(messageContent);
            }

            await MessageValidator.CheckAsync(entity);

            await Repository.InsertAsync(entity, autoSave: true);

            session.SetLastMessage(entity);

            await SessionGenerator.UpdateAsync(session);

            return entity;
        }

        public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IMessageContentEntity>> func)
            where TMessageInput : class, IMessageInput
        {
            var sender = await ChatObjectManager.GetItemByCacheAsync(input.SenderId);

            var receiver = await ChatObjectManager.GetItemByCacheAsync(input.ReceiverId);

            return await CreateMessageAsync(sender, receiver, async entity =>
            {
                entity.SetKey(input.KeyName, input.KeyValue);

                if (input.QuoteMessageId.HasValue)
                {
                    entity.SetQuoteMessage(await Repository.GetAsync(input.QuoteMessageId.Value));
                }
                return await func(entity);
            });
        }

        public virtual async Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IMessageContentEntity>> func)
        {
            var message = await CreateMessageAsync(input, func);

            //return null;

            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

            await SessionUnitManager.SetCacheListBySessionIdAsync(message.SessionId.Value);

            await ChatPusher.ExecuteAsync<ChatCommand>(output, input.IgnoreConnections);

            //var targetIdList = await ChatObjectResolver.GetIdListAsync(message);


            ////// get user id
            ////var userIdList = message.Session.UnitList
            ////    .Where(x => x.Owner.AppUserId != null)
            ////    .Select(x => x.Owner.AppUserId.Value)
            ////    .ToList();
            ////// get online user id
            ////var onlineUserIdList = userIdList.Where(x => true).ToList();
            ////// ,sessionUnit.OwnerId[chatObjectId]
            ////var sessionUnitList = message.Session.UnitList
            ////    .Where(x => x.Owner.AppUserId.HasValue && onlineUserIdList.Contains(x.Owner.AppUserId.Value)).ToList();

            ////await ChatPusher.ExecuteAsync<ChatCommand>(new SendDataEto(targetIdList, output), input.IgnoreConnections);

            return output;
        }

        public async Task<long> RollbackMessageAsync(Message message)
        {

            int HOURS = Config.AllowRollbackHours;

            var nowTime = Clock.Now;

            //var message = await Repository.GetAsync(messageId);

            //Assert.If(message.Sender != LoginInfo.UserId, $"无权限撤回别人消息！");

            Assert.If(nowTime > message.CreationTime.AddHours(HOURS), $"超过{HOURS}小时的消息不能被撤回！");

            message.Rollback(nowTime);

            await Repository.UpdateAsync(message, true);

            var targetIdList = await ChatObjectResolver.GetIdListAsync(message);

            await ChatPusher.ExecuteAsync<RollbackCommand>(new SendDataEto(targetIdList, message.Id));

            return 0;

        }

        public async Task<List<Message>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList)
        {
            var source = await Repository.GetAsync(sourceMessageId);

            Assert.If(source.IsRollbacked || source.RollbackTime != null, $"message already rollback：{sourceMessageId}");

            var sender = await ChatObjectManager.GetItemByCacheAsync(senderId);

            return await ForwardMessageAsync(source, sender, receiverIdList);
        }

        public async Task<List<Message>> ForwardMessageAsync(Message source, ChatObjectInfo sender, List<Guid> receiverIdList)
        {
            var isSelfSender = source.Sender.Id == sender.Id;

            Assert.If(!isSelfSender && source.MessageType == MessageTypes.Sound, $"Cannot forward voice messages from others");

            Assert.If(source.MessageType.IsDisabledForward(), $"The message type '{source.MessageType}' cannot be forwarded!");

            var messageContent = source.GetContent();

            Assert.NotNull(messageContent, $"MessageContent is null. Source message:{source}");

            var messageList = new List<Message>();

            foreach (var receiverId in receiverIdList.Distinct())
            {
                var receiver = await ChatObjectManager.GetItemByCacheAsync(receiverId);

                var newMessage = await CreateMessageAsync(sender, receiver, x =>
                {
                    x.SetForwardMessage(source);
                    return Task.FromResult(messageContent);
                });

                messageList.Add(newMessage);
            }
            return messageList;
        }






    }
}
