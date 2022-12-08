using IczpNet.Chat.MessageSections.Inputs;
using IczpNet.Chat.MessageSections.Outputs;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {

        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Action<Message> action = null) where TMessageInput : class, IMessageInput;
        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
        Task<TextMessageOuput> SendTextMessageAsync(TextMessageInput input);
    }
}
