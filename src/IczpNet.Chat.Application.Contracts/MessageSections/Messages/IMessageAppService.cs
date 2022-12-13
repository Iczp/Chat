using IczpNet.Chat.MessageSections.Templates;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageAppService
    {

        Task<List<Guid>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList);

        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
        Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input);
        Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input);
        Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input);
        Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input);
        Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input);
        Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input);
        Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input);
        Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input);
        Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input);
        Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input);
        Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input);
    }
}
