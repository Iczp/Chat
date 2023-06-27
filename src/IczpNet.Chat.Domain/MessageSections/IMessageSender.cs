using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IMessageSender
    {
        Task<MessageInfo<TextContentInfo>> SendTextAsync(SessionUnit senderSessionUnit, MessageInput<TextContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<CmdContentInfo>> SendCmdAsync(SessionUnit senderSessionUnit, MessageInput<CmdContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<LinkContentInfo>> SendLinkAsync(SessionUnit senderSessionUnit, MessageInput<LinkContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(SessionUnit senderSessionUnit, MessageInput<HtmlContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<ImageContentInfo>> SendImageAsync(SessionUnit senderSessionUnit, MessageInput<ImageContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<SoundContentInfo>> SendSoundAsync(SessionUnit senderSessionUnit, MessageInput<SoundContentInfo> input, SessionUnit receiverSessionUnit = null);
        Task<MessageInfo<VideoContentInfo>> SendVideoAsync(SessionUnit senderSessionUnit, MessageInput<VideoContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<FileContentInfo>> SendFileAsync(SessionUnit senderSessionUnit, MessageInput<FileContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<LocationContentInfo>> SendLocationAsync(SessionUnit senderSessionUnit, MessageInput<LocationContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(SessionUnit senderSessionUnit, MessageInput<ContactsContentInfo> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(SessionUnit senderSessionUnit, MessageInput<HistoryContentInput> input, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(SessionUnit senderSessionUnit, MessageInput<RedEnvelopeContentInput> input, SessionUnit receiverSessionUnit = null);

        //Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(MessageInput<ArticleContentInput> input);
    }
}
