using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageAppService
    {

        Task<MessageDto> SendTextMessageAsync(MessageInput<TextContentInfo> input);
    }
}
