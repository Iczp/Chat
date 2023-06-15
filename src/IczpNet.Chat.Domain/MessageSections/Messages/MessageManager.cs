using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
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
        protected IFollowManager FollowManager { get; }
        protected IBackgroundJobManager BackgroundJobManager { get; }
        protected ISettingProvider SettingProvider { get; }

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
            ISettingProvider settingProvider)
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
        }

        public virtual async Task<Message> CreateMessageAsync(
            SessionUnit senderSessionUnit,
            Func<Message, SessionUnitIncrementArgs, Task<IContentEntity>> action,
            SessionUnit receiverSessionUnit = null,
            long? quoteMessageId = null,
            List<Guid> remindList = null)
        {
            Assert.NotNull(senderSessionUnit, $"Unable to send message, senderSessionUnit is null");

            Assert.If(!senderSessionUnit.Setting.IsInputEnabled, $"Unable to send message, input status is disabled");

            //cache
            await SessionUnitManager.GetOrAddCacheListAsync(senderSessionUnit.SessionId.Value);

            var message = new Message(senderSessionUnit);

            var sessionUnitIncrementArgs = new SessionUnitIncrementArgs()
            {
                SessionId = senderSessionUnit.SessionId.Value,
                SenderSessionUnitId = senderSessionUnit.Id
            };

            //private message
            if (receiverSessionUnit != null)
            {
                var receiver = await ChatObjectManager.GetAsync(receiverSessionUnit.OwnerId);
                message.SetPrivateMessage(receiver);
            }

            //quote message
            if (quoteMessageId.HasValue)
            {
                message.SetQuoteMessage(await Repository.GetAsync(quoteMessageId.Value));
            }

            //remind List
            if (remindList != null)
            {
                sessionUnitIncrementArgs.RemindSessionUnitIdList = await GetRemindIdListAsync(senderSessionUnit, message, remindList);
            }

            // message content
            var messageContent = await action(message, sessionUnitIncrementArgs);

            Assert.NotNull(messageContent, $"Message content is null");

            message.SetMessageContent(messageContent);

            await MessageValidator.CheckAsync(message);

            //sessionUnitCount
            var sessionUnitCount = message.IsPrivate ? 2 : await SessionUnitManager.GetCountBySessionIdAsync(senderSessionUnit.SessionId.Value);

            message.SetSessionUnitCount(sessionUnitCount);

            await Repository.InsertAsync(message, autoSave: true);

            // session LastMessage
            await SessionRepository.UpdateLastMessageIdAsync(senderSessionUnit.SessionId.Value, message.Id);

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

            //var useBackgroundJobSenderMinSessionUnitCount = await SettingProvider.GetAsync<int>(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount);

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

        private async Task<List<Guid>> GetReminderIdListForTextContentAsync(SessionUnit senderSessionUnit, Message message)
        {
            var unitIdList = new List<Guid>();
            //@XXX
            if (message.MessageType != MessageTypes.Text)
            {
                return unitIdList;
            }
            //Guid.TryParse(message.Receiver, out Guid roomId);
            var textContent = message.GetContentEntity() as TextContent;

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
                if (senderSessionUnit.Setting.IsCreator)
                {
                    message.SetRemindAll();
                }
                return unitIdList;
            }

            unitIdList = await SessionUnitManager.GetIdListByNameAsync(senderSessionUnit.SessionId.Value, nameList);

            return unitIdList;
        }

        protected virtual async Task<List<Guid>> GetRemindIdListAsync(SessionUnit senderSessionUnit, Message message, List<Guid> remindIdList)
        {
            var finalRemindIdList = await GetReminderIdListForTextContentAsync(senderSessionUnit, message);

            if (remindIdList.IsAny())
            {
                finalRemindIdList = finalRemindIdList.Concat(remindIdList).Distinct().ToList();
            }

            if (!finalRemindIdList.Any())
            {
                return new List<Guid>();
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
                async (entity, args) => await Task.FromResult(contentEntity),
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

        public virtual async Task<Dictionary<string, long>> RollbackMessageAsync(Message message)
        {
            int allowRollbackHours = await SettingProvider.GetAsync<int>(ChatSettings.AllowRollbackHours);

            var nowTime = Clock.Now;

            //var message = await Repository.GetAsync(messageId);

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

        public virtual async Task<List<Message>> ForwardMessageAsync(Guid sessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList)
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

                Assert.If(currentSessionUnit.OwnerId != targetSessionUnit.OwnerId, $"[targetSessionUnitId:{targetSessionUnitId}] is fail.");

                var newMessage = await CreateMessageAsync(targetSessionUnit, async (x, args) =>
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


    }
}
