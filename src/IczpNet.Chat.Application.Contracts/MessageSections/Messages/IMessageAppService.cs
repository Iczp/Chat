using IczpNet.Chat.MessageSections.Inputs;
using IczpNet.Chat.MessageSections.Outputs;
using IczpNet.Chat.MessageSections.Templates;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageAppService
    {

        Task<TextMessageOuput> SendTextMessageAsync(TextMessageInput input);

        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
    }
}
