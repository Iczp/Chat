using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections;

public interface IMessageSender
{
    Task<MessageInfo<TextContentInfo>> SendTextAsync(SessionUnit senderSessionUnit, MessageInput<TextContentInfo> input);

    Task<MessageInfo<CmdContentInfo>> SendCmdAsync(SessionUnit senderSessionUnit, MessageInput<CmdContentInfo> input);

    Task<MessageInfo<LinkContentInfo>> SendLinkAsync(SessionUnit senderSessionUnit, MessageInput<LinkContentInfo> input);

    Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(SessionUnit senderSessionUnit, MessageInput<HtmlContentInfo> input);

    Task<MessageInfo<ImageContentInfo>> SendImageAsync(SessionUnit senderSessionUnit, MessageInput<ImageContentInfo> input);

    Task<MessageInfo<SoundContentInfo>> SendSoundAsync(SessionUnit senderSessionUnit, MessageInput<SoundContentInfo> input);
    Task<MessageInfo<VideoContentInfo>> SendVideoAsync(SessionUnit senderSessionUnit, MessageInput<VideoContentInfo> input);

    Task<MessageInfo<FileContentInfo>> SendFileAsync(SessionUnit senderSessionUnit, MessageInput<FileContentInfo> input);

    Task<MessageInfo<LocationContentInfo>> SendLocationAsync(SessionUnit senderSessionUnit, MessageInput<LocationContentInfo> input);

    Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(SessionUnit senderSessionUnit, MessageInput<ContactsContentInfo> input);

    Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(SessionUnit senderSessionUnit, MessageInput<HistoryContentInput> input);

    Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(SessionUnit senderSessionUnit, MessageInput<RedEnvelopeContentInput> input);

    //Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(MessageInput<ArticleContentInput> input);
}
