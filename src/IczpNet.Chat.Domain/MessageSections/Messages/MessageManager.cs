﻿using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages
{
    public partial class MessageManager : DomainService, IMessageManager
    {
        protected IObjectMapper ObjectMapper { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        protected IMessageRepository Repository { get; }
        protected IMessageValidator MessageValidator { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected IChatPusher ChatPusher { get; }
        protected ISessionRepository SessionRepository { get; }
        protected ISessionUnitSettingRepository SessionUnitSettingRepository { get; }
        protected IFollowManager FollowManager { get; }
        protected IBackgroundJobManager BackgroundJobManager { get; }
        protected ISettingProvider SettingProvider { get; }
        protected IRepository<MessageReminder> MessageReminderRepository { get; }
        protected ISessionGenerator SessionGenerator { get; }

        public MessageManager(
            IMessageRepository repository,
            IChatObjectManager chatObjectManager,
            IObjectMapper objectMapper,
            IMessageValidator messageValidator,
            IChatPusher chatPusher,
            ISessionUnitManager sessionUnitManager,
            IUnitOfWorkManager unitOfWorkManager,
            IFollowManager followManager,
            IBackgroundJobManager backgroundJobManager,
            ISessionRepository sessionRepository,
            ISettingProvider settingProvider,
            IRepository<MessageReminder> messageReminderRepository,
            ISessionUnitSettingRepository sessionUnitSettingRepository,
            ISessionGenerator sessionGenerator)
        {
            Repository = repository;
            ChatObjectManager = chatObjectManager;
            ObjectMapper = objectMapper;
            MessageValidator = messageValidator;
            ChatPusher = chatPusher;
            SessionUnitManager = sessionUnitManager;
            UnitOfWorkManager = unitOfWorkManager;
            FollowManager = followManager;
            BackgroundJobManager = backgroundJobManager;
            SessionRepository = sessionRepository;
            SettingProvider = settingProvider;
            MessageReminderRepository = messageReminderRepository;
            SessionUnitSettingRepository = sessionUnitSettingRepository;
            SessionGenerator = sessionGenerator;
        }

        protected virtual void TryToSetOwnerId<T, TKey>(T entity, TKey ownerId)
        {
            if (entity is IChatOwner<TKey>)
            {
                var propertyInfo = entity.GetType().GetProperty(nameof(IChatOwner<TKey>.OwnerId));

                if (propertyInfo == null || propertyInfo.GetSetMethod(true) == null)
                {
                    return;
                }

                propertyInfo.SetValue(entity, ownerId);
            }
        }

        public virtual async Task CreateSessionUnitByMessageAsync(SessionUnit senderSessionUnit)
        {
            //ShopKeeper
            if (senderSessionUnit.Destination.ObjectType == ChatObjectTypeEnums.ShopKeeper)
            {
                await SessionGenerator.AddShopWaitersIfNotContains(senderSessionUnit.Session, senderSessionUnit.Owner, senderSessionUnit.DestinationId.Value);
            }
            await Task.Yield();
        }
        public virtual async Task<Message> CreateMessageAsync(
            SessionUnit senderSessionUnit,
            Func<Message, Task<IContentEntity>> action,
            SessionUnit receiverSessionUnit = null,
            long? quoteMessageId = null,
            List<Guid> remindList = null)
        {

            Assert.If(!await SettingProvider.IsTrueAsync(ChatSettings.IsMessageSenderEnabled), $"MessageSender main switch is off.");

            Assert.NotNull(senderSessionUnit, $"Unable to send message, senderSessionUnit is null");

            Assert.If(!senderSessionUnit.Setting.IsInputEnabled, $"Unable to send message, input status is disabled,senderSessionUnitId:{senderSessionUnit.Id}");

            Assert.If(senderSessionUnit.Setting.MuteExpireTime > Clock.Now, $"Unable to send message,sessionUnit has been muted.senderSessionUnitId:{senderSessionUnit.Id}");

            // Create SessionUnit By Message
            await CreateSessionUnitByMessageAsync(senderSessionUnit);

            //cache
            await SessionUnitManager.GetOrAddCacheListAsync(senderSessionUnit.SessionId.Value);

            var message = new Message(senderSessionUnit)
            {
                CreationTime = Clock.Now
            };

            //senderSessionUnit.Setting.SetLastSendMessage(message);//并发时可能导致锁表

            //private message
            if (receiverSessionUnit != null)
            {
                var receiver = await ChatObjectManager.GetAsync(receiverSessionUnit.OwnerId);
                message.SetPrivateMessage(receiver);
            }

            //quote message
            if (quoteMessageId.HasValue && quoteMessageId.Value > 0)
            {
                var quoteMessage = await Repository.GetAsync(quoteMessageId.Value);

                Assert.If(quoteMessage.SessionId != senderSessionUnit.SessionId, $"QuoteMessageId:{quoteMessageId.Value} not in same session");

                message.SetQuoteMessage(quoteMessage);
            }

            // message content
            var messageContent = await action(message);

            //TryToSetOwnerId(messageContent, senderSessionUnit.SessionUnitId);
            messageContent.SetOwnerId(senderSessionUnit.OwnerId);

            Assert.NotNull(messageContent, $"Message content is null");

            message.SetMessageContent(messageContent);

            // Message Validator
            await MessageValidator.CheckAsync(message);

            // Args
            var sessionUnitIncrementArgs = new SessionUnitIncrementArgs()
            {
                SessionId = senderSessionUnit.SessionId.Value,
                SenderSessionUnitId = senderSessionUnit.Id
            };

            //remind List
            //if (remindList != null)
            //{
            sessionUnitIncrementArgs.RemindSessionUnitIdList = await ApplyRemindIdListAsync(senderSessionUnit, message, remindList);
            //}

            //sessionUnitCount
            var sessionUnitCount = message.IsPrivate ? 2 : await SessionUnitManager.GetCountBySessionIdAsync(senderSessionUnit.SessionId.Value);

            message.SetSessionUnitCount(sessionUnitCount);

            await Repository.InsertAsync(message, autoSave: true);

            // LastMessage
            await SessionRepository.UpdateLastMessageIdAsync(senderSessionUnit.SessionId.Value, message.Id);
            // Last send message
            await SessionUnitSettingRepository.UpdateLastSendMessageAsync(senderSessionUnit.Id, message.Id, message.CreationTime);
            ////以下可能导致锁表
            //await SessionUnitRepository.UpdateLastMessageIdAsync(senderSessionUnit.Id, message.Id);

            //senderSessionUnit.LastMessageId = message.Id;
            // private message
            if (message.IsPrivate || receiverSessionUnit != null)
            {
                sessionUnitIncrementArgs.PrivateBadgeSessionUnitIdList = new List<Guid>() { receiverSessionUnit.Id };
            }
            else
            {
                // Following
                sessionUnitIncrementArgs.FollowingSessionUnitIdList = await FollowManager.GetFollowerIdListAsync(senderSessionUnit.Id);
            }

            sessionUnitIncrementArgs.LastMessageId = message.Id;
            sessionUnitIncrementArgs.IsRemindAll = message.IsRemindAll;
            sessionUnitIncrementArgs.MessageCreationTime = message.CreationTime;

            //await CurrentUnitOfWork.SaveChangesAsync();

            if (await ShouldbeBackgroundJobAsync(senderSessionUnit, message))
            {
                var jobId = await BackgroundJobManager.EnqueueAsync(sessionUnitIncrementArgs);

                Logger.LogInformation($"SessionUnitIncrement backgroupJobId:{jobId},args:{sessionUnitIncrementArgs}");
            }
            else
            {
                await SessionUnitManager.IncremenetAsync(sessionUnitIncrementArgs);
            }

            return message;
        }

        protected virtual async Task<bool> ShouldbeBackgroundJobAsync(SessionUnit senderSessionUnit, Message message)
        {
            await Task.Yield();

            return BackgroundJobManager.IsAvailable();

            //var useBackgroundJobSenderMinSessionUnitCount = await SettingProvider.GetWalletAsync<int>(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount);

            //return BackgroundJobManager.IsAvailable() && !message.IsPrivate && message.SessionUnitCount > useBackgroundJobSenderMinSessionUnitCount;

            ////return false;
        }

        protected virtual async Task BatchUpdateSessionUnitAsync(SessionUnit senderSessionUnit, Message message)
        {
            Logger.LogInformation($"BatchUpdateSessionUnitAsync");

            await SessionUnitManager.UpdateCachesAsync(senderSessionUnit, message);

            if (await ShouldbeBackgroundJobAsync(senderSessionUnit, message))
            {
                var jobId = await BackgroundJobManager.EnqueueAsync(new UpdateStatsForSessionUnitArgs()
                {
                    SenderSessionUnitId = senderSessionUnit.Id,
                    MessageId = message.Id,
                });

                Logger.LogInformation($"JobId:{jobId}");

                return;
            }
            //
            await SessionUnitManager.BatchUpdateAsync(senderSessionUnit, message);
        }


        [GeneratedRegex("@([^@ ]+) ?")]
        private static partial Regex RemindNameRegex();

        private async Task<List<Guid>> ApplyReminderIdListForTextContentAsync(SessionUnit senderSessionUnit, Message message)
        {
            var unitIdList = new List<Guid>();
            //@XXX
            if (message.MessageType != MessageTypes.Text)
            {
                return unitIdList;
            }
            //Guid.TryParse(message.Receiver, out Guid roomId);
            var textContent = message.GetContentEntity() as TextContent;

            Assert.If(textContent == null, "TextContent is null");

            var text = textContent.Text;

            var reg = RemindNameRegex();

            //例如我想提取 @中的NAME值
            var match = reg.Match(text);

            var nameList = new List<string>();

            for (var i = 0; i < match.Groups.Count; i++)
            {
                string value = match.Groups[i].Value;

                if (!value.IsNullOrWhiteSpace() && !value.StartsWith("@"))
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
                //creator or manager
                //if (senderSessionUnit.Setting.IsCreator)
                //{
                message.SetRemindAll();
                //}
                return unitIdList;
            }

            unitIdList = await SessionUnitManager.GetIdListByNameAsync(senderSessionUnit.SessionId.Value, nameList);

            return unitIdList;
        }

        protected virtual async Task<List<Guid>> ApplyRemindIdListAsync(SessionUnit senderSessionUnit, Message message, List<Guid> remindIdList)
        {
            var finalRemindIdList = await ApplyReminderIdListForTextContentAsync(senderSessionUnit, message);

            if (remindIdList.IsAny())
            {
                finalRemindIdList = finalRemindIdList.Concat(remindIdList).Distinct().ToList();
            }

            if (!finalRemindIdList.Any())
            {
                return [];
            }

            message.SetReminder(finalRemindIdList, ReminderTypes.Normal);

            //await SessionUnitRepository.IncrementRemindMeCountAsync(message.CreationTime, finalRemindIdList);

            return finalRemindIdList;
        }

        public async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
            SessionUnit senderSessionUnit,
            MessageInput<TContentInfo> input,
            SessionUnit receiverSessionUnit = null)
            where TContentInfo : IContentInfo
            where TContentEntity : IContentEntity
        {
            var messageContent = ObjectMapper.Map<TContentInfo, TContentEntity>(input.Content);
            return await SendAsync<TContentInfo, TContentEntity>(senderSessionUnit, input, messageContent, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
            SessionUnit senderSessionUnit,
            MessageInput input,
            TContentEntity contentEntity,
            SessionUnit receiverSessionUnit = null)
            where TContentInfo : IContentInfo
            where TContentEntity : IContentEntity
        {
            var message = await CreateMessageAsync(senderSessionUnit,
                async (entity) => await Task.FromResult(contentEntity),
                quoteMessageId: input.QuoteMessageId,
                remindList: input.RemindList);

            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

            //var output = new MessageInfo<TContentInfo>() { Id = message.Id };

            if (message.IsPrivate)
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

        public virtual async Task<Dictionary<string, long>> RollbackAsync(Message message)
        {
            int allowRollbackHours = await SettingProvider.GetAsync<int>(ChatSettings.AllowRollbackHours);

            var nowTime = Clock.Now;

            //var message = await Repository.GetWalletAsync(messageId);

            //Assert.If(message.Sender != LoginInfo.UserId, $"无权限撤回别人消息！");

            Assert.If(nowTime > message.CreationTime.AddHours(allowRollbackHours), $"超过{allowRollbackHours}小时的消息不能被撤回！");

            message.Rollback(nowTime);

            //await Repository.UpdateAsync(message, true);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            return await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, new RollbackMessageCommandPayload
            {
                MessageId = message.Id,
            });
        }

        public virtual async Task<List<Message>> ForwardAsync(Guid sessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList)
        {
            var currentSessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            Assert.If(!currentSessionUnit.Setting.IsEnabled, $"Current session unit disabled.", nameof(currentSessionUnit.Setting.IsEnabled));

            var sourceMessage = await Repository.GetAsync(sourceMessageId);

            Assert.If(sourceMessage.IsRollbacked || sourceMessage.RollbackTime != null, $"message already rollback：{sourceMessageId}", nameof(currentSessionUnit.Setting.IsEnabled));

            Assert.If(sourceMessage.IsDisabledForward, $"MessageType:'{sourceMessage.MessageType}' is disabled forward");

            Assert.If(sourceMessage.IsPrivate, $"Private messages cannot be forward");

            Assert.If(currentSessionUnit.SessionId != sourceMessage.SessionId, $"The sender and message are not in the same session, messageSessionId:{sourceMessage.SessionId}", nameof(currentSessionUnit.SessionId));

            var messageContent = sourceMessage.GetTypedContentEntity();

            Assert.NotNull(messageContent, $"MessageContent is null. Source message:{sourceMessage}");

            var messageList = new List<Message>();

            var args = new List<(Guid, MessageInfo<object>)>();

            foreach (var targetSessionUnitId in targetSessionUnitIdList.Distinct())
            {
                var targetSessionUnit = await SessionUnitManager.GetAsync(targetSessionUnitId);

                Assert.If(!targetSessionUnit.Setting.IsEnabled, $"Target session unit disabled,id:{targetSessionUnit.Id}");

                Assert.If(!targetSessionUnit.Setting.IsInputEnabled, $"Target session unit input state is disabled,id:{targetSessionUnit.Id}");

                Assert.If(currentSessionUnit.OwnerId != targetSessionUnit.OwnerId, $"[TargetSessionUnitId:{targetSessionUnitId}] is fail.");

                var newMessage = await CreateMessageAsync(targetSessionUnit, async (x) =>
                {
                    x.SetForwardMessage(sourceMessage);
                    await Task.Yield();
                    return messageContent;
                });
                messageList.Add(newMessage);

                var output = ObjectMapper.Map<Message, MessageInfo<dynamic>>(newMessage);

                //var output = new MessageInfo<object>() { Id = newMessage.Id };

                args.Add((newMessage.SessionId.Value, output));
            }

            //push
            foreach (var arg in args)
            {
                await ChatPusher.ExecuteBySessionIdAsync(arg.Item1, arg.Item2, null);
            }

            return messageList;
        }

        public Task<bool> IsRemindAsync(long messageId, Guid sessionUnitId)
        {
            return MessageReminderRepository.AnyAsync(x => x.MessageId == messageId && x.SessionUnitId == sessionUnitId);
        }
    }
}
