using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections;

public interface IMessageSender
{
    Task<MessageInfo<TextContentInfo>> SendTextAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<TextContentInfo> input);

    Task<MessageInfo<CmdContentInfo>> SendCmdAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<CmdContentInfo> input);

    Task<MessageInfo<LinkContentInfo>> SendLinkAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<LinkContentInfo> input);

    Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<HtmlContentInfo> input);

    Task<MessageInfo<ImageContentInfo>> SendImageAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<ImageContentInfo> input);

    Task<MessageInfo<SoundContentInfo>> SendSoundAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<SoundContentInfo> input);
    Task<MessageInfo<VideoContentInfo>> SendVideoAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<VideoContentInfo> input);

    Task<MessageInfo<FileContentInfo>> SendFileAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<FileContentInfo> input);

    Task<MessageInfo<LocationContentInfo>> SendLocationAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<LocationContentInfo> input);

    Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<ContactsContentInfo> input);

    Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<HistoryContentInput> input);

    Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<RedEnvelopeContentInput> input);

    //Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(MessageInput<ArticleContentInput> input);
}
