using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.ContentInputs;
using IczpNet.Chat.MessageSections.ContentOutputs;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {
        Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Action<Message> action = null);
        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Action<Message> action = null) where TMessageInput : class, IMessageInput;
        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, IMessageContentEntity content) where TMessageInput : class, IMessageInput;

        Task<List<Message>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList);
        Task<List<Message>> ForwardMessageAsync(Message source, ChatObject sender, List<Guid> receiverIdList);

        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
        Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input);
    }
}
