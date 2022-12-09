using IczpNet.Chat.MessageSections.ContentInputs;
using IczpNet.Chat.MessageSections.ContentOutputs;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {

        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Action<Message> action = null) where TMessageInput : class, IMessageInput;
        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, IMessageContentEntity content) where TMessageInput : class, IMessageInput;

        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
        Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input);
    }
}
