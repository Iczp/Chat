using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages
{
    public partial class MessageManager : DomainService, IMessageManager
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
        protected ISessionRepository SessionRepository { get; }
        protected IFollowManager FollowManager { get; }
        protected IBackgroundJobManager BackgroundJobManager { get; }

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
            ISessionUnitRepository sessionUnitRepository,
            IFollowManager followManager,
            IBackgroundJobManager backgroundJobManager,
            ISessionRepository sessionRepository)
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
            FollowManager = followManager;
            BackgroundJobManager = backgroundJobManager;
            SessionRepository = sessionRepository;
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

        public virtual async Task<Message> CreateMessageBySessionUnitAsync(SessionUnit senderSessionUnit, Func<Message, Task> action, SessionUnit receiverSessionUnit = null)
        {
            Assert.NotNull(senderSessionUnit, $"Unable to send message, senderSessionUnit is null");

            Assert.If(!senderSessionUnit.IsInputEnabled, $"Unable to send message, input status is disabled");

            var entity = new Message(senderSessionUnit);

            if (action != null)
            {
                await action(entity);
            }

            await MessageValidator.CheckAsync(entity);

            //sessionUnitCount
            var sessionUnitCount = entity.IsPrivate ? 2 : await SessionUnitManager.GetCountAsync(senderSessionUnit.SessionId.Value);

            entity.SetSessionUnitCount(sessionUnitCount);

            await Repository.InsertAsync(entity, autoSave: true);

            // session LastMessage
            //var session = await SessionRepository.GetAsync(senderSessionUnit.SessionId.Value);

            //session.SetLastMessage(entity);

            //await SessionRepository.UpdateAsync(session, autoSave: true);

            // sender SessionUnit LastMessage
            senderSessionUnit.SetLastMessage(entity);

            await SessionUnitRepository.UpdateAsync(senderSessionUnit, autoSave: true);

            // private message
            if (entity.IsPrivate || receiverSessionUnit != null)
            {
                receiverSessionUnit.SetLastMessage(entity);
                receiverSessionUnit.SetPrivateBadge(receiverSessionUnit.PrivateBadge + 1);
                await SessionUnitRepository.UpdateAsync(receiverSessionUnit, autoSave: true);
            }

            // Following
            await SessionUnitManager.UpdateFollowingCountAsync(senderSessionUnit, entity);

            //Batch Update SessionUnit
            await BatchUpdateSessionUnitAsync(senderSessionUnit, entity);
            //
            return entity;
        }

        protected virtual bool ShouldbeBackgroundJob(SessionUnit senderSessionUnit, Message message)
        {
            //return BackgroundJobManager.IsAvailable() && !message.IsPrivate;
            return false;
        }

        protected virtual async Task BatchUpdateSessionUnitAsync(SessionUnit senderSessionUnit, Message message)
        {
            if (ShouldbeBackgroundJob(senderSessionUnit, message))
            {
                await CurrentUnitOfWork.SaveChangesAsync();

                await SessionUnitManager.BatchUpdateCacheAsync(senderSessionUnit, message);

                var jobId = await BackgroundJobManager.EnqueueAsync(new UpdateStatsForSessionUnitArgs()
                {
                    SenderSessionUnitId = senderSessionUnit.Id,
                    MessageId = message.Id,
                });

                Logger.LogInformation($"JobId:{jobId}");

                return;
            }

            await SessionUnitManager.BatchUpdateAsync(senderSessionUnit, message);

            Logger.LogInformation($"BatchUpdateSessionUnitAsync");

            //SaveChangesAsync
            await CurrentUnitOfWork.SaveChangesAsync();

            await SessionUnitManager.BatchUpdateCacheAsync(senderSessionUnit, message);
        }


        [GeneratedRegex("@([^ ]+) ?")]
        private static partial Regex RemindNameRegex();

        private async Task<List<Guid>> GetRemindIdListForTextContentAsync(SessionUnit senderSessionUnit, Message message)
        {
            var unitIdList = new List<Guid>();
            //@XXX
            if (message.MessageType != MessageTypes.Text)
            {
                return unitIdList;
            }
            //Guid.TryParse(message.Receiver, out Guid roomId);
            var textContent = message.GetContent() as TextContent;

            var text = textContent.Text;

            var reg = RemindNameRegex();

            //例如我想提取 @中的NAME值
            var match = reg.Match(text);

            var nameList = new List<string>();

            for (var i = 0; i < match.Groups.Count; i++)
            {
                string value = match.Groups[i].Value;

                if (!value.IsNullOrWhiteSpace())
                {
                    nameList.Add(value);
                }
            }
            if (!nameList.Any())
            {
                return unitIdList;
            }
            var textList = new string[] { "所有人", "everyone" };

            if (nameList.Any(x => textList.Contains(x)))
            {
                //creator and manager
                //if (senderSessionUnit.IsCreator)
                //{
                //    message.SetRemindAll();
                //}
                message.SetRemindAll();

                return unitIdList;
            }

            unitIdList = await SessionUnitManager.GetIdListByNameAsync(nameList);

            return unitIdList;
        }

        protected virtual async Task SetRemindAsync(SessionUnit senderSessionUnit, Message message, List<Guid> remindIdList)
        {
            var reminIdList = await GetRemindIdListForTextContentAsync(senderSessionUnit, message);

            var finalRemindIdList = reminIdList.Concat(remindIdList).Distinct().ToList();

            if (!finalRemindIdList.Any())
            {
                return;
            }

            message.SetReminder(finalRemindIdList, ReminderTypes.Normal);

            await SessionUnitRepository.BatchUpdateRemindMeCountAsync(message.CreationTime, finalRemindIdList);
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

                entity.SetMessageContent(messageContent);

                await SetRemindAsync(senderSessionUnit, entity, input.RemindList);
            });

            //var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

            var output = new MessageInfo<TContentInfo>() { Id = message.Id };

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

            var messageContent = sourceMessage.GetTypedContent();

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
                    x.SetMessageContent(messageContent);
                    await Task.CompletedTask;
                });
                messageList.Add(newMessage);

                var output = ObjectMapper.Map<Message, MessageInfo<object>>(newMessage);

                await ChatPusher.ExecuteBySessionIdAsync(newMessage.SessionId.Value, output, null);
            }
            return messageList;
        }


    }
}
