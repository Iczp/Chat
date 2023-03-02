using IczpNet.Chat.MessageSections.Messages;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IReminderManager
    {
        Task SetRemindAsync(Message message);
    }
}
