using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageManager : DomainService, IMessageManager
    {
        protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }
        protected IRepository<Message, Guid> Repository { get; }

        public MessageManager(
            IRepository<Message, Guid> repository,
            IRepository<ChatObject, Guid> chatObjectRepository)
        {
            Repository = repository;
            ChatObjectRepository = chatObjectRepository;
        }

        protected virtual async Task<Message> SendMessageAsync<TContentInfo, TContentInput>(MessageInput<TContentInput> input)
            where TContentInfo : class
            where TContentInput : class
        {
            var sender = await ChatObjectRepository.GetAsync(input.SenderId);

            var receiver = await ChatObjectRepository.GetAsync(input.ReceiverId);

            var entity = new Message(sender, receiver, input.KeyName, input.KeyValue);

            if (input.QuoteMessageId.HasValue)
            {
                entity.SetQuoteMessage(await Repository.GetAsync(input.QuoteMessageId.Value));
            }
            //entity.SetMessageContent()
            return await Repository.InsertAsync(entity, autoSave: true);
        }
        protected virtual Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo, TContentInput>(TContentInput input)
            where TContentInfo : class
            where TContentInput : class
        {
            throw new NotImplementedException();
        }

        public virtual Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            throw new NotImplementedException();
        }
    }
}
