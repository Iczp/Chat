using IczpNet.Chat.MessageSections.Messages;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections;

public interface IMessageValidator
{
    Task CheckAsync(Message entity);
}
