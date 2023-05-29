using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.FavoriteRecorders;

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
        protected IReadedRecorderManager ReadedRecorderManager { get; }
        protected IOpenedRecorderManager OpenedRecorderManager { get; }
        protected IFavoritedRecorderManager FavoriteManager { get; }

        public MessageAppService(
            IMessageRepository repository,
            IMessageManager messageManager,
            IMessageSender messageSender,
            ISessionUnitManager sessionUnitManager,
            IReadedRecorderManager readedRecorderManager,
            IOpenedRecorderManager openedRecorderManager,
            IFavoritedRecorderManager favoriteManager) : base(repository)
        {
            MessageSender = messageSender;
            MessageManager = messageManager;
            SessionUnitManager = sessionUnitManager;
            ReadedRecorderManager = readedRecorderManager;
            OpenedRecorderManager = openedRecorderManager;
            FavoriteManager = favoriteManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public Task<List<EnumDto>> GetDisabledForwardListAsync()
        {
            var result = MessageExtentions.DisabledForwardList
                .Select(x => new EnumDto()
                {
                    Name = Enum.GetName(x),
                    Description = x.GetDescription(),
                    Value = (int)x
                })
                .ToList();

            return Task.FromResult(result);
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
        //public Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        //{
        //    return MessageSender.SendRedEnvelopeMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        //{
        //    return MessageSender.SendHtmlMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        //{
        //    return MessageSender.SendImageMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        //{
        //    return MessageSender.SendSoundMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        //{
        //    return MessageSender.SendVideoMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        //{
        //    return MessageSender.SendFileMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        //{
        //    return MessageSender.SendLocationMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        //{
        //    return MessageSender.SendContactsMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        //{
        //    return MessageSender.SendLinkMessageAsync(input);
        //}

        //[HttpPost]
        //public Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        //{
        //    return MessageSender.SendHistoryMessageAsync(input);
        //}

        [HttpPost]
        public async Task<Dictionary<string, long>> RollbackMessageAsync(long messageId)
        {
            var message = await Repository.GetAsync(messageId);
            return await MessageManager.RollbackMessageAsync(message);
        }

        [HttpPost]
        public async Task<MessageInfo<TextContentInfo>> SendTextAsync(MessageSendInput<TextContentInfo> input)
        {
            var sessionunit = await SessionUnitManager.GetAsync(input.SessionUnitId);

            return await MessageSender.SendTextAsync(sessionunit, input);
        }

        [HttpPost]
        public async Task<List<MessageDto>> ForwardMessageAsync(Guid currentSessionUnitId, long messageId, List<Guid> targetSessionUnitIdList)
        {
            var messageList = await MessageManager.ForwardMessageAsync(currentSessionUnitId, messageId, targetSessionUnitIdList);

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
