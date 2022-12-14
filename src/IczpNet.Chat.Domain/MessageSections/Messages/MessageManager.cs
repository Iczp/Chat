using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageManager : DomainService, IMessageManager
    {
        protected Type ObjectMapperContext { get; set; }
        protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
        ObjectMapperContext == null
            ? provider.GetRequiredService<IObjectMapper>()
            : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));

        //protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }

        protected IChatObjectManager ChatObjectManager { get; }
        protected IRepository<Message, Guid> Repository { get; }
        protected ISessionIdGenerator SessionIdGenerator => LazyServiceProvider.LazyGetRequiredService<ISessionIdGenerator>();
        protected IMessageChannelResolver MessageChannelResolver => LazyServiceProvider.LazyGetRequiredService<IMessageChannelResolver>();
        protected IRedEnvelopeGenerator RedEnvelopeGenerator { get; }
        protected IMessageChatObjectResolver MessageChatObjectResolver { get; }
        protected IContentResolver ContentResolver { get; }


        public MessageManager(
            IRepository<Message, Guid> repository,
            IRedEnvelopeGenerator redEnvelopeGenerator,
            IMessageChatObjectResolver messageChatObjectResolver,
            IChatObjectManager chatObjectManager,
            IContentResolver contentResolver)
        {
            Repository = repository;
            RedEnvelopeGenerator = redEnvelopeGenerator;
            MessageChatObjectResolver = messageChatObjectResolver;
            ChatObjectManager = chatObjectManager;
            ContentResolver = contentResolver;
        }
        public virtual async Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Action<Message> action = null)
        {
            var messageChannel = await MessageChannelResolver.MakeAsync(sender, receiver);

            var sessionId = await SessionIdGenerator.MakeAsync(messageChannel, sender, receiver);

            var entity = new Message(GuidGenerator.Create(), messageChannel, sender, receiver, sessionId);

            action?.Invoke(entity);
            //entity.SetMessageContent(input.Content);

            return await Repository.InsertAsync(entity, autoSave: true);
        }

        public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Action<Message> action = null)

            where TMessageInput : class, IMessageInput
        {
            var sender = await ChatObjectManager.GetAsync(input.SenderId);

            var receiver = await ChatObjectManager.GetAsync(input.ReceiverId);

            return await CreateMessageAsync(sender, receiver, entity =>
            {
                entity.SetKey(input.KeyName, input.KeyValue);

                if (input.QuoteMessageId.HasValue)
                {
                    entity.SetQuoteMessage(Repository.GetAsync(input.QuoteMessageId.Value).Result);
                }
                action?.Invoke(entity);
            });
        }

        /// <summary>
        /// 设置【@我】提醒
        /// </summary>
        /// <param name="message"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        protected async Task SetRemindAsync(Message message, Room room)
        {
            //@XXX
            if (message.MessageType != MessageTypes.Text)
            {
                return;
            }
            //Guid.TryParse(message.Receiver, out Guid roomId);
            var textContent = message.TextContentList.FirstOrDefault();

            var text = textContent.Text;

            var reg = new Regex("@([^ ]+) ?");

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
                return;
            }
            if (nameList.IndexOf("所有人") != -1)
            {
                message.SetRemindAll();

                return;
            }
            var chatObjectIdList = await ChatObjectManager.GetIdListByNameAsync(nameList);

            if (!chatObjectIdList.Any())
            {
                return;
            }
            var roomId = message.Receiver;

            //验证被@的人是否在群里
            var memberChatObjectIdList = room.RoomMemberList.Where(x => chatObjectIdList.Contains(x.OwnerId)).Select(x => x.OwnerId).ToList();

            if (memberChatObjectIdList.Any())
            {
                message.SetRemindChatObject(memberChatObjectIdList);
            }
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

        public virtual async Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Action<Message> action = null)
            where TContentInfo : class, IMessageContentInfo
            //where TContent : class, IMessageContentEntity
        {
            var message = await CreateMessageAsync(input, action);

            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);

            var chatObjectList = await MessageChatObjectResolver.GetChatObjectListAsync(message);
            //push
            return await Task.FromResult(output);
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
                    x.SetMessageContent((IMessageContentEntity)messageContent);
                    x.SetForwardMessage(source);
                });

                messageList.Add(newMessage);
            }
            return messageList;
        }

        public virtual async Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<CmdContentInfo, CmdContent>(input.Content);
            return await SendMessageAsync<CmdContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<TextContentInfo, TextContent>(input.Content);
            return await SendMessageAsync<TextContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<HtmlContentInfo, HtmlContent>(input.Content);
            return await SendMessageAsync<HtmlContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<ImageContentInfo, ImageContent>(input.Content);
            return await SendMessageAsync<ImageContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<SoundContentInfo, SoundContent>(input.Content);
            return await SendMessageAsync<SoundContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<VideoContentInfo, VideoContent>(input.Content);
            return await SendMessageAsync<VideoContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<FileContentInfo, FileContent>(input.Content);
            return await SendMessageAsync<FileContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<LocationContentInfo, LocationContent>(input.Content);
            return await SendMessageAsync<LocationContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<ContactsContentInfo, ContactsContent>(input.Content);
            return await SendMessageAsync<ContactsContentInfo>(input, x => x.SetMessageContent(messageContent));
        }

        public virtual async Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<LinkContentInfo, LinkContent>(input.Content);
            return await SendMessageAsync<LinkContentInfo>(input, x => x.SetMessageContent(messageContent));
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
                .Select(x => $"{x.Sender.Name}:{x.GetTypedContent().GetBody()}")
                .ToArray()
                .JoinAsString("\n");

            var messageContent = new HistoryContent(GuidGenerator.Create(), $"聊天记录({selectedMessageList.Count})", description);

            return await SendMessageAsync<HistoryContentOutput>(input, message =>
            {
                var historyDetailList = selectedMessageList.Select(x => new HistoryMessage(messageContent, message)).ToList();

                messageContent.SetHistoryMessageList(historyDetailList);

                //message.HistoryContentList.Add(historyContent);
                message.SetMessageContent(messageContent);
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

            return await SendMessageAsync<RedEnvelopeContentOutput>(input, x => x.SetMessageContent(messageContent));
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
            return await SendMessageAsync<TextContentInfo>(input, x => x.SetMessageContent(content));
        }
    }
}
