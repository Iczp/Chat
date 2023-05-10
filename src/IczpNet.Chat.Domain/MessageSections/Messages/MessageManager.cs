using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageManager : DomainService, IMessageManager
    {
        protected IObjectMapper ObjectMapper { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        protected IMessageRepository Repository { get; }
        protected ISessionGenerator SessionGenerator { get; }
        protected IMessageValidator MessageValidator { get; }
        protected IChatObjectResolver ChatObjectResolver { get; }
        protected IContentResolver ContentResolver { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected IUnitOfWork CurrentUnitOfWork => UnitOfWorkManager?.Current;

        protected ChatOption Config { get; }
        protected IChatPusher ChatPusher { get; }
        protected ISessionUnitRepository SessionUnitRepository { get; }

        public MessageManager(
            IMessageRepository repository,
            IChatObjectResolver messageChatObjectResolver,
            IChatObjectManager chatObjectManager,
            IContentResolver contentResolver,
            IObjectMapper objectMapper,
            IMessageValidator messageValidator,
            ISessionGenerator sessionIdGenerator,
            IOptions<ChatOption> options,
            IChatPusher chatPusher,
            ISessionUnitManager sessionUnitManager,
            IUnitOfWorkManager unitOfWorkManager,
            ISessionUnitRepository sessionUnitRepository)
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
            UnitOfWorkManager = unitOfWorkManager;
            SessionUnitRepository = sessionUnitRepository;
        }

        //public virtual async Task<Message> CreateMessageAsync(IChatObject sender, IChatObject receiver, Func<Message, Task<IMessageContentEntity>> func)
        //{
        //    var session = await SessionGenerator.MakeAsync(sender, receiver);

        //    var entity = new Message(sender, receiver, session);

        //    if (func != null)
        //    {
        //        var messageContent = await func(entity);
        //        entity.SetMessageContent(messageContent);
        //    }

        //    await MessageValidator.CheckAsync(entity);

        //    var sessionUnitCount = entity.IsPrivate ? 2 : await SessionUnitManager.GetCountAsync(session.Id);

        //    entity.SetSessionUnitCount(sessionUnitCount);

        //    await Repository.InsertAsync(entity, autoSave: true);

        //    session.SetLastMessage(entity);

        //    await SessionGenerator.UpdateAsync(session);

        //    return entity;
        //}

        //public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IMessageContentEntity>> func)
        //    where TMessageInput : class, IMessageInput
        //{
        //    var sender = await ChatObjectManager.GetItemByCacheAsync(input.SenderId);

        //    var receiver = await ChatObjectManager.GetItemByCacheAsync(input.ReceiverId);

        //    return await CreateMessageAsync(sender, receiver, async entity =>
        //    {
        //        entity.SetKey(input.KeyName, input.KeyValue);

        //        if (input.QuoteMessageId.HasValue)
        //        {
        //            entity.SetQuoteMessage(await Repository.GetAsync(input.QuoteMessageId.Value));
        //        }
        //        return await func(entity);
        //    });
        //}

        //public virtual async Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IMessageContentEntity>> func)
        //{
        //    var message = await CreateMessageAsync(input, func);

        //    var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

        //    await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, output, input.IgnoreConnections);

        //    return output;
        //}

        public virtual async Task<Message> CreateMessageBySessionUnitAsync(SessionUnit senderSessionUnit, Func<Message, Task<IMessageContentEntity>> getContentEntity, SessionUnit receiverSessionUnit = null)
        {
            Assert.NotNull(senderSessionUnit, $"Unable to send message, senderSessionUnit is null");

            Assert.If(!senderSessionUnit.IsInputEnabled, $"Unable to send message, input status is disabled");

            var session = senderSessionUnit.Session;

            var entity = new Message(senderSessionUnit);

            if (getContentEntity != null)
            {
                var messageContent = await getContentEntity(entity);
                entity.SetMessageContent(messageContent);
            }

            await MessageValidator.CheckAsync(entity);

            var sessionUnitCount = entity.IsPrivate ? 2 : await SessionUnitManager.GetCountAsync(senderSessionUnit.SessionId.Value);

            entity.SetSessionUnitCount(sessionUnitCount);

            await Repository.InsertAsync(entity, autoSave: true);

            session.SetLastMessage(entity);

            //following
            await UpdateFollowingCountAsync(senderSessionUnit, entity);

            //IsRemindAll @everyone
            await UpdateRemindAllCountAsync(senderSessionUnit, entity);

            // update LastMessageId
            await UpdateLastMessageIdAsync(senderSessionUnit, receiverSessionUnit, entity);

            //update badge
            await UpdateBadgeAsync(senderSessionUnit, entity);

            //
            await CurrentUnitOfWork.SaveChangesAsync();

            return entity;
        }

        private async Task UpdateBadgeAsync(SessionUnit senderSessionUnit, Message entity)
        {
            if (entity.IsPrivate)
            {
                await SessionUnitRepository.BatchUpdatePrivateBadgeAsync(senderSessionUnit.SessionId.Value, entity.CreationTime, receiverSessionUnitId: senderSessionUnit.Id);
            }
            else
            {
                await SessionUnitRepository.BatchUpdatePublicBadgeAsync(senderSessionUnit.SessionId.Value, entity.CreationTime, ignoreSessionUnitId: senderSessionUnit.Id);
            }
        }

        private async Task UpdateLastMessageIdAsync(SessionUnit senderSessionUnit, SessionUnit receiverSessionUnit, Message message)
        {
            List<Guid> sessionUnitList = null;

            if (receiverSessionUnit != null)
            {
                sessionUnitList = new List<Guid>() { senderSessionUnit.Id, receiverSessionUnit.Id, };
            }

            await SessionUnitRepository.BatchUpdateLastMessageIdAsync(senderSessionUnit.SessionId.Value, message.Id, sessionUnitList);
        }

        private async Task UpdateRemindAllCountAsync(SessionUnit senderSessionUnit, Message message)
        {
            if (message.IsRemindAll)
            {
                await SessionUnitRepository.BatchUpdateRemindAllCountAsync(senderSessionUnit.SessionId.Value, message.CreationTime, ignoreSessionUnitId: senderSessionUnit.Id);
            }
        }

        private async Task UpdateFollowingCountAsync(SessionUnit senderSessionUnit, Message message)
        {
            if (senderSessionUnit.OwnerFollowList.Any())
            {
                var destinationSessionUnitIdList = senderSessionUnit.OwnerFollowList.Select(x => x.DestinationId).ToList();
                await SessionUnitRepository.BatchUpdateFollowingCountAsync(senderSessionUnit.SessionId.Value, message.CreationTime, destinationSessionUnitIdList: destinationSessionUnitIdList);
            }
        }

        public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContent>(SessionUnit senderSessionUnit, MessageSendInput<TContentInfo> input, SessionUnit receiverSessionUnit = null)
            where TContentInfo : IMessageContentInfo
            where TContent : IMessageContentEntity
        {
            var message = await CreateMessageBySessionUnitAsync(senderSessionUnit, async entity =>
            {
                entity.SetKey(input.KeyName, input.KeyValue);

                if (input.QuoteMessageId.HasValue)
                {
                    entity.SetQuoteMessage(await Repository.GetAsync(input.QuoteMessageId.Value));
                }

                if (receiverSessionUnit != null)
                {
                    var receiver = await ChatObjectManager.GetAsync(receiverSessionUnit.OwnerId);
                    entity.SetPrivateMessage(receiver);
                }
                var messageContent = ObjectMapper.Map<TContentInfo, TContent>(input.Content);

                //remind sessionUnitId
                if (input.RemindList.IsAny())
                {
                    entity.SetReminder(input.RemindList, ReminderTypes.Normal);
                    await SessionUnitRepository.BatchUpdateRemindMeCountAsync(entity.CreationTime, input.RemindList);
                }

                return await Task.FromResult(messageContent);
            });

            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

            if (receiverSessionUnit != null)
            {
                await ChatPusher.ExecutePrivateAsync(new List<SessionUnit>()
                {
                    senderSessionUnit, receiverSessionUnit
                }, output, input.IgnoreConnections);
            }
            else
            {
                await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, output, input.IgnoreConnections);
            }
            return output;
        }

        public virtual async Task<Dictionary<string, long>> RollbackMessageAsync(Message message)
        {
            int HOURS = Config.AllowRollbackHours;

            var nowTime = Clock.Now;

            //var message = await Repository.GetAsync(messageId);

            //Assert.If(message.Sender != LoginInfo.UserId, $"无权限撤回别人消息！");

            Assert.If(nowTime > message.CreationTime.AddHours(HOURS), $"超过{HOURS}小时的消息不能被撤回！");

            message.Rollback(nowTime);

            //await Repository.UpdateAsync(message, true);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, new RollbackMessageCommandPayload
            {
                MessageId = message.Id,
            });
        }

        //public virtual async Task<List<Message>> ForwardMessageAsync(long sourceMessageId, long senderId, List<long> receiverIdList)
        //{
        //    var source = await Repository.GetAsync(sourceMessageId);

        //    Assert.If(source.IsRollbacked || source.RollbackTime != null, $"message already rollback：{sourceMessageId}");

        //    var sender = await ChatObjectManager.GetItemByCacheAsync(senderId);

        //    return await ForwardMessageAsync(source, sender, receiverIdList);
        //}

        //public virtual async Task<List<Message>> ForwardMessageAsync(Message source, IChatObject sender, List<long> receiverIdList)
        //{
        //    var isSelfSender = source.Sender.Id == sender.Id;

        //    Assert.If(!isSelfSender && source.MessageType == MessageTypes.Sound, $"Cannot forward voice messages from others");

        //    Assert.If(source.MessageType.IsDisabledForward(), $"The message type '{source.MessageType}' cannot be forwarded!");

        //    var messageContent = source.GetContent();

        //    Assert.NotNull(messageContent, $"MessageContent is null. Source message:{source}");

        //    var messageList = new List<Message>();

        //    foreach (var receiverId in receiverIdList.Distinct())
        //    {
        //        var receiver = await ChatObjectManager.GetItemByCacheAsync(receiverId);

        //        var newMessage = await CreateMessageAsync(sender, receiver, x =>
        //        {
        //            x.SetForwardMessage(source);
        //            return Task.FromResult(messageContent);
        //        });

        //        messageList.Add(newMessage);
        //    }
        //    return messageList;
        //}

        public virtual async Task<List<Message>> ForwardMessageAsync(Guid currentSessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList)
        {
            var currentSessionUnit = await SessionUnitManager.GetAsync(currentSessionUnitId);

            Assert.If(!currentSessionUnit.IsEnabled, $"Current session unit disabled.", nameof(currentSessionUnit.IsEnabled));

            var sourceMessage = await Repository.GetAsync(sourceMessageId);

            Assert.If(sourceMessage.IsRollbacked || sourceMessage.RollbackTime != null, $"message already rollback：{sourceMessageId}", nameof(currentSessionUnit.IsEnabled));

            Assert.If(sourceMessage.IsPrivate, $"Private messages cannot be forwarded");

            Assert.If(currentSessionUnit.SessionId != sourceMessage.SessionId, $"The sender and message are not in the same session, messageSessionId:{sourceMessage.SessionId}", nameof(currentSessionUnit.SessionId));

            var messageContent = sourceMessage.GetContent();

            Assert.NotNull(messageContent, $"MessageContent is null. Source message:{sourceMessage}");

            var messageList = new List<Message>();

            foreach (var targetSessionUnitId in targetSessionUnitIdList.Distinct())
            {
                var targetSessionUnit = await SessionUnitManager.GetAsync(targetSessionUnitId);

                Assert.If(!targetSessionUnit.IsEnabled, $"Target session unit disabled,id:{targetSessionUnit.Id}");

                Assert.If(!targetSessionUnit.IsInputEnabled, $"Target session unit input state is disabled,id:{targetSessionUnit.Id}");

                Assert.If(currentSessionUnit.OwnerId != targetSessionUnit.OwnerId, $"[targetSessionUnitId:{targetSessionUnitId}] is fail.");

                var newMessage = await CreateMessageBySessionUnitAsync(targetSessionUnit, async x =>
                {
                    x.SetForwardMessage(sourceMessage);

                    return await Task.FromResult(messageContent);
                });
                messageList.Add(newMessage);

                var output = ObjectMapper.Map<Message, MessageInfo<object>>(newMessage);

                await ChatPusher.ExecuteBySessionIdAsync(newMessage.SessionId.Value, output, null);
            }
            return messageList;
        }
    }
}
