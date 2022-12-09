using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections;
using Microsoft.Extensions.DependencyInjection;
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
        protected Type ObjectMapperContext { get; set; }
        protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
        ObjectMapperContext == null
            ? provider.GetRequiredService<IObjectMapper>()
            : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));

        protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }
        protected IRepository<Message, Guid> Repository { get; }
        protected ISessionIdGenerator SessionIdGenerator => LazyServiceProvider.LazyGetRequiredService<ISessionIdGenerator>();
        protected IMessageChannelGenerator MessageChannelGenerator => LazyServiceProvider.LazyGetRequiredService<IMessageChannelGenerator>();


        public MessageManager(
            IRepository<Message, Guid> repository,
            IRepository<ChatObject, Guid> chatObjectRepository)
        {
            Repository = repository;
            ChatObjectRepository = chatObjectRepository;
        }
        public virtual async Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Action<Message> action = null)
        {
            var messageChannel = await MessageChannelGenerator.MakeAsync(sender, receiver);

            var sessionId = await SessionIdGenerator.MakeAsync(messageChannel, sender, receiver);

            var entity = new Message(GuidGenerator.Create(), messageChannel, sender, receiver, sessionId);

            action?.Invoke(entity);
            //entity.SetMessageContent(input.Content);

            return await Repository.InsertAsync(entity, autoSave: true);
        }

        public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Action<Message> action = null)

            where TMessageInput : class, IMessageInput
        {
            var sender = await ChatObjectRepository.GetAsync(input.SenderId);

            var receiver = await ChatObjectRepository.GetAsync(input.ReceiverId);

            return await CreateMessageAsync(sender, receiver, async entity =>
            {
                entity.SetKey(input.KeyName, input.KeyValue);

                if (input.QuoteMessageId.HasValue)
                {
                    entity.SetQuoteMessage(await Repository.GetAsync(input.QuoteMessageId.Value));
                }
                action?.Invoke(entity);
            });
        }

        public virtual Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, IMessageContentEntity content)
            where TMessageInput : class, IMessageInput
        {
            return CreateMessageAsync(input, x =>
            {
                x.SetMessageContent(content);
                //x.SetContentJson();
            });
        }

        public virtual async Task<MessageInfo<TContentInfo>> SendMessageAsync<TContent, TContentInfo>(MessageInput input, IMessageContentEntity content)
            where TContentInfo : class, IMessageContentInfo
            where TContent : class, IMessageContentEntity
        {
            //var content = ObjectMapper.Map<TContentInfo, TContent>(input.Content);
            var message = await CreateMessageAsync(input, content);
            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);
            //push
            return await Task.FromResult(output);
        }

        public async Task<List<Message>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList)
        {
            var source = await Repository.GetAsync(sourceMessageId);

            Assert.If(source.IsRollbacked || source.RollbackTime != null, $"message already rollback：{sourceMessageId}");

            var sender = await ChatObjectRepository.GetAsync(senderId);

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
                var receiver = await ChatObjectRepository.GetAsync(receiverId);

                var newMessage = await CreateMessageAsync(sender, receiver, x =>
                {
                    x.SetMessageContent((IMessageContentEntity)messageContent);
                });
                messageList.Add(newMessage);
            }
            return messageList;
        }

        public virtual async Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<CmdContentInfo, CmdContent>(input.Content);
            return await SendMessageAsync<CmdContent, CmdContentInfo>(input, messageContent);
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<TextContentInfo, TextContent>(input.Content);
            return await SendMessageAsync<TextContent, TextContentInfo>(input, messageContent);
        }

        public virtual async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        {
            var messageContent = ObjectMapper.Map<RedEnvelopeContentInput, RedEnvelopeContent>(input.Content);
            return await SendMessageAsync<RedEnvelopeContent, RedEnvelopeContentOutput>(input, messageContent);
        }

        public virtual async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput messageInput, RedEnvelopeContentInput contentInput)
        {
            var messageContent = ObjectMapper.Map<RedEnvelopeContentInput, RedEnvelopeContent>(contentInput);
            return await SendMessageAsync<RedEnvelopeContent, RedEnvelopeContentOutput>(messageInput, messageContent);
        }
    }
}
