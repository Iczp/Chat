using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Inputs;
using IczpNet.Chat.MessageSections.Outputs;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.DependencyInjection;
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

            var entity = new Message(GuidGenerator.Create(), sender, receiver);

            entity.SetMessageType(input.MessageType);

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
            return CreateMessageAsync(input, x => x.SetMessageContent(content));
        }

        public Task<TContent> MapToContentAsync<TContentInfo, TContent>(TContentInfo info)
            where TContentInfo : class, IMessageContentInfo
            where TContent : class, IMessageContent
        {
            var content = ObjectMapper.Map<TContentInfo, TContent>(info);
            return Task.FromResult(content);
        }

        public virtual async Task<TMessageOuput> SendMessageAsync<TMessageOuput>(Message message)
            where TMessageOuput : class, IMessageOuput
        {
            var output = ObjectMapper.Map<Message, TMessageOuput>(message);
            return await Task.FromResult(output);
        }

        public virtual async Task<MessageInfo<TContentInfo>> PushMessageAsync<TContentInfo>(Message message)
            where TContentInfo : class, IMessageContentInfo
        {


            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);
            return await Task.FromResult(output);
        }

        public virtual async Task<TextMessageOuput> SendTextMessageAsync(TextMessageInput input)
        {
            var content = ObjectMapper.Map<TextContentInfo, TextContent>(input.Content);
            var message = await CreateMessageAsync(input, content);
            await SendMessageAsync<TextMessageOuput>(message);
            return await SendMessageAsync<TextMessageOuput>(message);
        }

        public virtual async Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return await SendAsync<CmdContentInfo, CmdContent>(input);
        }


        public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContent>(MessageInput<TContentInfo> input)
            where TContentInfo : class, IMessageContentInfo
            where TContent : class, IMessageContent
        {
            var content = ObjectMapper.Map<TContentInfo, TContent>(input.Content);
            var message = await CreateMessageAsync(input, content);
            var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);
            return await Task.FromResult(output);

        }
    }
}
