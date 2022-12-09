using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.ContentInputs;
using IczpNet.Chat.MessageSections.ContentOutputs;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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

        public virtual async Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Action<Message> action = null)

            where TMessageInput : class, IMessageInput
        {
            var sender = await ChatObjectRepository.GetAsync(input.SenderId);

            var receiver = await ChatObjectRepository.GetAsync(input.ReceiverId);

            var messageChannel = await MessageChannelGenerator.MakeAsync(sender, receiver);

            var sessionId = await SessionIdGenerator.MakeAsync(messageChannel, sender, receiver);

            var entity = new Message(GuidGenerator.Create(), messageChannel, sender, receiver, sessionId);

            entity.SetKey(input.KeyName, input.KeyValue);

            if (input.QuoteMessageId.HasValue)
            {
                entity.SetQuoteMessage(await Repository.GetAsync(input.QuoteMessageId.Value));
            }

            action?.Invoke(entity);
            //entity.SetMessageContent(input.Content);

            return await Repository.InsertAsync(entity, autoSave: true);
        }

        public virtual Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, IMessageContent content)
            where TMessageInput : class, IMessageInput
        {
            return CreateMessageAsync(input, x =>
            {
                x.SetMessageContent(content);
                //x.SetContentJson();
            });
        }

        public virtual async Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo, TContent>(MessageInput<TContentInfo> input)
            where TContentInfo : class, IMessageContentInfo
            where TContent : class, IMessageContent
        {
            var content = ObjectMapper.Map<TContentInfo, TContent>(input.Content);
            var message = await CreateMessageAsync(input, content);
            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);
            return await Task.FromResult(output);
        }

        public virtual async Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return await SendMessageAsync<CmdContentInfo, CmdContent>(input);
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            return await SendMessageAsync<TextContentInfo, TextContent>(input);
        }
    }
}
