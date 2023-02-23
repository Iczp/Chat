using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageServices
{
    public class MessageAppService
        : CrudChatAppService<
        Message,
        MessageDetailDto,
        MessageDto,
        Guid,
        MessageGetListInput,
        MessageCreateInput,
        MessageUpdateInput>,
        IMessageAppService
    {

        protected IMessageSender ChatSender { get; }
        protected IMessageManager MessageManager { get; }

        public MessageAppService(
            IRepository<Message, Guid> repository,
            IMessageManager messageManager,
            IMessageSender chatSender) : base(repository)
        {
            ChatSender = chatSender;
            MessageManager = messageManager;
        }

        public async Task<List<Guid>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList)
        {
            var messageList = await MessageManager.ForwardMessageAsync(sourceMessageId, senderId, receiverIdList);
            return messageList.Select(x => x.Id).ToList();
        }

        public virtual Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            return ChatSender.SendTextMessageAsync(input);
        }

        public Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return ChatSender.SendCmdMessageAsync(input);
        }

        public Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        {
            return ChatSender.SendRedEnvelopeMessageAsync(input);
        }

        public Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        {
            return ChatSender.SendHtmlMessageAsync(input);
        }

        public Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        {
            return ChatSender.SendImageMessageAsync(input);
        }

        public Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        {
            return ChatSender.SendSoundMessageAsync(input);
        }

        public Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        {
            return ChatSender.SendVideoMessageAsync(input);
        }

        public Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        {
            return ChatSender.SendFileMessageAsync(input);
        }

        public Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        {
            return ChatSender.SendLocationMessageAsync(input);
        }

        public Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        {
            return ChatSender.SendContactsMessageAsync(input);
        }

        public Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        {
            return ChatSender.SendLinkMessageAsync(input);
        }

        public Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        {
            return ChatSender.SendHistoryMessageAsync(input);
        }

        public async Task<Dictionary<string, long>> RollbackMessageAsync(Guid messageId)
        {
            var message = await Repository.GetAsync(messageId);
            return await MessageManager.RollbackMessageAsync(message);
        }
    }
}
