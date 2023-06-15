using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageServices
{
    public class MessageSenderAppService : ChatAppService, IMessageSenderAppService
    {
        protected IMessageRepository Repository { get; }
        protected IMessageSender MessageSender { get; }
        protected IMessageManager MessageManager { get; }

        public MessageSenderAppService(
            IMessageRepository repository,
            IMessageManager messageManager,
            IMessageSender messageSender) 
        {
            Repository = repository;
            MessageSender = messageSender;
            MessageManager = messageManager;
        }

        //[HttpPost]
        //[RemoteService(false)]
        //public async Task<List<long>> ForwardMessageAsync(long sourceMessageId, long senderId, List<long> receiverIdList)
        //{
        //    var messageList = await MessageManager.ForwardMessageAsync(sourceMessageId, senderId, receiverIdList);
        //    return messageList.Select(x => x.Id).ToList();
        //}

        //[HttpPost]
        //public virtual Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        //{
        //    return MessageSender.SendTextMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        //{
        //    return MessageSender.SendCmdMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(MessageInput<RedEnvelopeContentInput> input)
        //{
        //    return MessageSender.SendRedEnvelopeAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(MessageInput<HtmlContentInfo> input)
        //{
        //    return MessageSender.SendHtmlAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<ImageContentInfo>> SendImageAsync(MessageInput<ImageContentInfo> input)
        //{
        //    return MessageSender.SendImageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<SoundContentInfo>> SendSoundAsync(MessageInput<SoundContentInfo> input)
        //{
        //    return MessageSender.SendSoundAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<VideoContentInfo>> SendVideoAsync(MessageInput<VideoContentInfo> input)
        //{
        //    return MessageSender.SendVideoAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<FileContentInfo>> SendFileAsync(MessageInput<FileContentInfo> input)
        //{
        //    return MessageSender.SendFileAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<LocationContentInfo>> SendLocationAsync(MessageInput<LocationContentInfo> input)
        //{
        //    return MessageSender.SendLocationAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(MessageInput<ContactsContentInfo> input)
        //{
        //    return MessageSender.SendContactsAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<LinkContentInfo>> SendLinkAsync(MessageInput<LinkContentInfo> input)
        //{
        //    return MessageSender.SendLinkAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        //{
        //    return MessageSender.SendHistoryMessageAsync(input);
        //}

        /// <inheritdoc/>
        [HttpPost]
        public async Task<Dictionary<string, long>> RollbackMessageAsync(long messageId)
        {
            var message = await Repository.GetAsync(messageId);
            return await MessageManager.RollbackMessageAsync(message);
        }

        /// <inheritdoc/>
        [HttpPost]
        public async Task<MessageInfo<TextContentInfo>> SendTextAsync(Guid sessionUnitId, MessageInput<TextContentInfo> input)
        {
            var sessionunit = await SessionUnitManager.GetAsync(sessionUnitId);

            return await MessageSender.SendTextAsync(sessionunit, input);
        }

        /// <inheritdoc/>
        [HttpPost]
        public async Task<List<MessageDto>> ForwardMessageAsync(Guid sessionUnitId, long messageId, List<Guid> targets)
        {
            var messageList = await MessageManager.ForwardMessageAsync(sessionUnitId, messageId, targets);

            return ObjectMapper.Map<List<Message>, List<MessageDto>>(messageList);
        }

        //[HttpPost]
        //public async Task<MessageDto> UpdateRecorderAsync(long messageId)
        //{
        //    var message = await Repository.GetAsync(messageId);

        //    message.ReadedCount = await ReadedRecorderManager.GetCountByMessageIdAsync(messageId);

        //    message.OpenedCount = await OpenedRecorderManager.GetCountByMessageIdAsync(messageId);

        //    message.FavoritedCount = await FavoritedRecorderManager.GetCountByMessageIdAsync(messageId);

        //    await Repository.UpdateAsync(message, autoSave: true);

        //    return await MapToGetOutputDtoAsync(message);
        //}
    }
}
