using IczpNet.Chat.MessageSections.Templates;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {

        Task<Message> SendTextMessageAsync(MessageInput<TextContentInfo> input);
    }
}
