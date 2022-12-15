using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RoomSections.Rooms;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IReminderManager
    {
        Task SetRemindAsync(Message message, Room room);
    }
}
