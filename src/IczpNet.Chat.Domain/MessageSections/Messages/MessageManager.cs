using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
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
        protected IChatObjectResolver MessageChatObjectResolver { get; }
        protected IContentResolver ContentResolver { get; }
        protected ChatOption Config { get; }

        public MessageManager(
            IRepository<Message, Guid> repository,
            IChatObjectResolver messageChatObjectResolver,
            IChatObjectManager chatObjectManager,
            IContentResolver contentResolver,
            IObjectMapper objectMapper,
            IMessageValidator messageValidator,
            ISessionGenerator sessionIdGenerator,
            IOptions<ChatOption> options)
        {
            Repository = repository;
            MessageChatObjectResolver = messageChatObjectResolver;
            ChatObjectManager = chatObjectManager;
            ContentResolver = contentResolver;
            ObjectMapper = objectMapper;
            MessageValidator = messageValidator;
            SessionGenerator = sessionIdGenerator;
            Config = options.Value;
        }

        public virtual async Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Func<Message, Task<IMessageContentEntity>> func)
        {
            var session = await SessionGenerator.MakeAsync(sender, receiver);

            var entity = new Message(GuidGenerator.Create(), sender, receiver, session);

            if (func != null)
            {
                var messageContent = await func(entity);
                entity.SetMessageContent(messageContent);
            }

            await MessageValidator.CheckAsync(entity);

            return await Repository.InsertAsync(entity, autoSave: true);
        }

        public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IMessageContentEntity>> func)
            where TMessageInput : class, IMessageInput
        {
            var sender = await ChatObjectManager.GetAsync(input.SenderId);

            var receiver = await ChatObjectManager.GetAsync(input.ReceiverId);

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

            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

            var chatObjectList = await MessageChatObjectResolver.GetListAsync(message);
            //push
            return await Task.FromResult(output);
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

            // push
            //var targetUserIdList = await MessageChatObjectResolver.GetChatObjectIdListAsync(message);

            //return await PusherFactory.SendToAsync<RollbackCommand>(x => targetUserIdList.Contains(x.UserId), rollbackMessageCommandMessage);

            return 0;

        }

        public async Task<List<Message>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList)
        {
            var source = await Repository.GetAsync(sourceMessageId);

            Assert.If(source.IsRollbacked || source.RollbackTime != null, $"message already rollback：{sourceMessageId}");

            var sender = await ChatObjectManager.GetAsync(senderId);

            return await ForwardMessageAsync(source, sender, receiverIdList);
        }

        public async Task<List<Message>> ForwardMessageAsync(Message source, ChatObject sender, List<Guid> receiverIdList)
        {
            var isSelfSender = source.Sender.Id == sender.Id;

            Assert.If(!isSelfSender && source.MessageType == MessageTypes.Sound, $"Cannot forward voice messages from others");

            Assert.If(source.MessageType.IsDisabledForward(), $"The message type '{source.MessageType}' cannot be forwarded!");

            var messageContent = source.GetContent();

            Assert.NotNull(messageContent, $"MessageContent is null. Source message:{source}");

            var messageList = new List<Message>();

            foreach (var receiverId in receiverIdList.Distinct())
            {
                var receiver = await ChatObjectManager.GetAsync(receiverId);

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
