using IczpNet.Chat.MessageSections.Messages.Dtos;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageChangeAppService
{
    Task<MessageChangePullDto> PullAsync(MessageChangePullInput input);

    Task<MessageChangeAckDto> AckAsync(MessageChangeAckInput input);
}
