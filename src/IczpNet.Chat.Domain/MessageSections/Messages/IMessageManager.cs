using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {
        Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Func<Message, Task<IMessageContentEntity>> func);
        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IMessageContentEntity>> func) where TMessageInput : class, IMessageInput;

        Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IMessageContentEntity>> func);

        Task<List<Message>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList);
        Task<List<Message>> ForwardMessageAsync(Message source, ChatObject sender, List<Guid> receiverIdList);

        Task<long> RollbackMessageAsync(Message message);

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
        //Task<MessageInfo<ArticleContentOutput>> SendArticleContentMessageAsync(MessageInput<ArticleContentInput> input);
    }
}
