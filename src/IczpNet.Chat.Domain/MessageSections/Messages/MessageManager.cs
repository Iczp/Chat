using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections;
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
        protected IChannelResolver ChannelResolver { get; }
        protected IMessageValidator MessageValidator { get; }
        protected IRedEnvelopeGenerator RedEnvelopeGenerator { get; }
        protected IChatObjectResolver MessageChatObjectResolver { get; }
        protected IContentResolver ContentResolver { get; }
        protected ChatOption Config { get; }

        public MessageManager(
            IRepository<Message, Guid> repository,
            IRedEnvelopeGenerator redEnvelopeGenerator,
            IChatObjectResolver messageChatObjectResolver,
            IChatObjectManager chatObjectManager,
            IContentResolver contentResolver,
            IObjectMapper objectMapper,
            IChannelResolver messageChannelResolver,
            IMessageValidator messageValidator,
            ISessionGenerator sessionIdGenerator,
            IOptions<ChatOption> options)
        {
            Repository = repository;
            RedEnvelopeGenerator = redEnvelopeGenerator;
            MessageChatObjectResolver = messageChatObjectResolver;
            ChatObjectManager = chatObjectManager;
            ContentResolver = contentResolver;
            ObjectMapper = objectMapper;
            ChannelResolver = messageChannelResolver;
            MessageValidator = messageValidator;
            SessionGenerator = sessionIdGenerator;
            Config = options.Value;
        }

        public virtual async Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Func<Message, Task<IMessageContentEntity>> func)
        {
            var messageChannel = await ChannelResolver.GetAsync(sender, receiver);

            var session = await SessionGenerator.MakeAsync(messageChannel, sender, receiver);

            var entity = new Message(GuidGenerator.Create(), messageChannel, sender, receiver, session);

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

        //public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, IMessageContentEntity content)
        //    where TMessageInput : class, IMessageInput
        //{
        //    return await CreateMessageAsync(input, x =>
        //    {
        //        x.SetMessageContent(content);
        //        //x.SetContentJson();
        //    });
        //}

        public virtual async Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IMessageContentEntity>> func)
        //where TContentInfo : class, IMessageContentInfo
        //where TContent : class, IMessageContentEntity
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

        public virtual Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return SendMessageAsync<CmdContentInfo>(input, async x => await Task.FromResult(ObjectMapper.Map<CmdContentInfo, CmdContent>(input.Content)));
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<TextContentInfo, TextContent>(input.Content);
            return await SendMessageAsync<TextContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<HtmlContentInfo, HtmlContent>(input.Content);
            return await SendMessageAsync<HtmlContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<ImageContentInfo, ImageContent>(input.Content);
            return await SendMessageAsync<ImageContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<SoundContentInfo, SoundContent>(input.Content);
            return await SendMessageAsync<SoundContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<VideoContentInfo, VideoContent>(input.Content);
            return await SendMessageAsync<VideoContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<FileContentInfo, FileContent>(input.Content);
            return await SendMessageAsync<FileContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<LocationContentInfo, LocationContent>(input.Content);
            return await SendMessageAsync<LocationContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<ContactsContentInfo, ContactsContent>(input.Content);
            return await SendMessageAsync<ContactsContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<LinkContentInfo, LinkContent>(input.Content);
            return await SendMessageAsync<LinkContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        {
            var messageIdList = input.Content.MessageIdList;

            var selectedMessageList = (await Repository.GetQueryableAsync())
                .Where(x => messageIdList.Contains(x.Id))
                .Where(x => !x.IsRollbacked || x.RollbackTime == null)
                .OrderBy(x => x.AutoId)
                //.Select(x => x.Id)
                .ToList();

            var description = selectedMessageList.Take(3)
                .Select(x => $"{x.Sender?.Name}:{x.GetTypedContent()?.GetBody()}")
                .ToArray()
                .JoinAsString("\n");

            var messageContent = new HistoryContent(GuidGenerator.Create(), $"聊天记录({selectedMessageList.Count})", description);

            return await SendMessageAsync<HistoryContentOutput>(input, async message =>
            {
                var historyDetailList = selectedMessageList.Select(x => new HistoryMessage(messageContent, message)).ToList();

                messageContent.SetHistoryMessageList(historyDetailList);

                //message.HistoryContentList.Add(historyContent);
                return await Task.FromResult(messageContent);
            });
        }

        public virtual async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        {
            var content = input.Content;
            var messageContent = new RedEnvelopeContent(
                id: GuidGenerator.Create(),
                grantMode: content.GrantMode,
                amount: content.Amount,
                count: content.Count,
                totalAmount: content.TotalAmount,
                text: content.Text
                );

            var redEnvelopeUnitList = await RedEnvelopeGenerator.MakeAsync(content.GrantMode, messageContent.Id, content.Amount, content.Count, content.TotalAmount);

            messageContent.SetRedEnvelopeUnitList(redEnvelopeUnitList);

            return await SendMessageAsync<RedEnvelopeContentOutput>(input, async x => await Task.FromResult(messageContent));
        }

        protected virtual IContentProvider GetContentProvider(string providerName)
        {
            var providerType = ContentResolver.GetProviderTypeOrDefault(providerName);
            return LazyServiceProvider.LazyGetService(providerType) as IContentProvider;
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextTemplateMessageAsync(MessageInput<TextContentInfo> input)
        {
            var provider = GetContentProvider(TextContentProvider.Name);
            var content = await provider.Create<TextContentInfo, TextContent>(input.Content);
            return await SendMessageAsync<TextContentInfo>(input, async x => await Task.FromResult(content));
        }


    }
}
