using IczpNet.Chat.BaseAppServices;
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

        protected IMessageManager MessageManager { get; set; }

        public MessageAppService(
            IRepository<Message, Guid> repository,
            IMessageManager messageManager) : base(repository)
        {
            MessageManager = messageManager;
        }

        public async Task<List<Guid>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList)
        {
            var messageList = await MessageManager.ForwardMessageAsync(sourceMessageId, senderId, receiverIdList);
            return messageList.Select(x => x.Id).ToList();
        }

        public virtual Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            return MessageManager.SendTextMessageAsync(input);
        }

        public Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return MessageManager.SendCmdMessageAsync(input);
        }

        public Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        {
            return MessageManager.SendRedEnvelopeMessageAsync(input);
        }

        public Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        {
            return MessageManager.SendHtmlMessageAsync(input);
        }

        public Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        {
            return MessageManager.SendImageMessageAsync(input);
        }

        public Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        {
            return MessageManager.SendSoundMessageAsync(input);
        }

        public Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        {
            return MessageManager.SendVideoMessageAsync(input);
        }

        public Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        {
            return MessageManager.SendFileMessageAsync(input);
        }

        public Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        {
            return MessageManager.SendLocationMessageAsync(input);
        }

        public Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        {
            return MessageManager.SendContactsMessageAsync(input);
        }

        public Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        {
            return MessageManager.SendLinkMessageAsync(input);
        }

        public Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        {
            return MessageManager.SendHistoryMessageAsync(input);
        }
    }
}
