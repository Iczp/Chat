using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageServices
{
    public class MessageAppService
        : CrudChatAppService<
        Message,
        MessageDetailDto,
        MessageDto,
        long,
        MessageGetListInput,
        MessageCreateInput,
        MessageUpdateInput>,
        IMessageAppService
    {

        protected IMessageSender MessageSender { get; }
        protected IMessageManager MessageManager { get; }
        protected ISessionUnitManager SessionUnitManager { get; }


        public MessageAppService(
            IMessageRepository repository,
            IMessageManager messageManager,
            IMessageSender messageSender,
            ISessionUnitManager sessionUnitManager) : base(repository)
        {
            MessageSender = messageSender;
            MessageManager = messageManager;
            SessionUnitManager = sessionUnitManager;
        }

        public async Task<List<long>> ForwardMessageAsync(long sourceMessageId, long senderId, List<long> receiverIdList)
        {
            var messageList = await MessageManager.ForwardMessageAsync(sourceMessageId, senderId, receiverIdList);
            return messageList.Select(x => x.Id).ToList();
        }

        public virtual Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            return MessageSender.SendTextMessageAsync(input);
        }

        public Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return MessageSender.SendCmdMessageAsync(input);
        }

        public Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        {
            return MessageSender.SendRedEnvelopeMessageAsync(input);
        }

        public Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        {
            return MessageSender.SendHtmlMessageAsync(input);
        }

        public Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        {
            return MessageSender.SendImageMessageAsync(input);
        }

        public Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        {
            return MessageSender.SendSoundMessageAsync(input);
        }

        public Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        {
            return MessageSender.SendVideoMessageAsync(input);
        }

        public Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        {
            return MessageSender.SendFileMessageAsync(input);
        }

        public Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        {
            return MessageSender.SendLocationMessageAsync(input);
        }

        public Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        {
            return MessageSender.SendContactsMessageAsync(input);
        }

        public Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        {
            return MessageSender.SendLinkMessageAsync(input);
        }

        public Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        {
            return MessageSender.SendHistoryMessageAsync(input);
        }

        public async Task<Dictionary<string, long>> RollbackMessageAsync(long messageId)
        {
            var message = await Repository.GetAsync(messageId);
            return await MessageManager.RollbackMessageAsync(message);
        }

        public async Task<MessageInfo<TextContentInfo>> SendTextAsync(MessageSendInput<TextContentInfo> input)
        {
            var sessionunit = await SessionUnitManager.GetAsync(input.SessionUnitId);

            return await MessageSender.SendTextAsync(sessionunit, input);
        }
    }
}
