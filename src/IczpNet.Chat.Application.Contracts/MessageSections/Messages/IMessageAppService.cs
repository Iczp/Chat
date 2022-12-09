using IczpNet.Chat.MessageSections.Templates;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageAppService
    {
        Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input);

        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
    }
}
