using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageSenderAppService
    {
        //Task<MessageDto> UpdateRecorderAsync(long messageId);

        //Task<List<long>> ForwardMessageAsync(long sourceMessageId, long senderId, List<long> receiverIdList);

        //Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
        //Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input);
        //Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input);
        //Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input);
        //Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input);
        //Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input);
        //Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input);
        //Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input);
        //Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input);
        //Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input);
        //Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input);
        //Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input);
        Task<Dictionary<string, long>> RollbackMessageAsync(long messageId);


        Task<MessageInfo<TextContentInfo>> SendTextAsync(Guid sessionUnitId, MessageSendInput<TextContentInfo> input);

        /// <summary>
        /// 转发消息
        /// </summary>
        /// <param name="sessionUnitId">当前会话单元</param>
        /// <param name="messageId">原消息</param>
        /// <param name="targets">目标</param>
        /// <returns></returns>
        Task<List<MessageDto>> ForwardMessageAsync(Guid sessionUnitId, long messageId, List<Guid> targets);
    }
}
